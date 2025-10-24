using System.Collections.Concurrent;

namespace SoNice.Api.Middleware;

/// <summary>
/// Rate limiting middleware - matches Node.js loginLimiter logic exactly
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly ConcurrentDictionary<string, RateLimitInfo> _attempts = new();
    private readonly int _maxAttempts;
    private readonly TimeSpan _windowSize;
    private readonly TimeSpan _blockDuration;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _maxAttempts = configuration.GetValue<int>("RateLimit:MaxAttempts", 5);
        _windowSize = TimeSpan.FromMinutes(configuration.GetValue<int>("RateLimit:WindowMinutes", 15));
        _blockDuration = TimeSpan.FromMinutes(configuration.GetValue<int>("RateLimit:BlockMinutes", 15));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only apply rate limiting to login endpoints exactly like Node.js
        if (IsLoginEndpoint(context.Request.Path))
        {
            var clientId = GetClientIdentifier(context);
            var now = DateTime.UtcNow;

            // Clean up old entries
            CleanupOldEntries(now);

            // Check if client is blocked
            if (_attempts.TryGetValue(clientId, out var rateLimitInfo))
            {
                if (rateLimitInfo.IsBlocked && now < rateLimitInfo.BlockedUntil)
                {
                    _logger.LogWarning($"Rate limit exceeded for client {clientId}");
                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsync("Quá nhiều lần thử đăng nhập. Vui lòng thử lại sau 15 phút.");
                    return;
                }

                // Reset if block period has passed
                if (rateLimitInfo.IsBlocked && now >= rateLimitInfo.BlockedUntil)
                {
                    _attempts.TryRemove(clientId, out _);
                }
            }

            // Process request
            await _next(context);

            // Check if login failed and update rate limit
            if (context.Response.StatusCode == 401 || context.Response.StatusCode == 400)
            {
                UpdateRateLimit(clientId, now);
            }
            else if (context.Response.StatusCode == 200)
            {
                // Login successful, remove rate limit info
                _attempts.TryRemove(clientId, out _);
            }
        }
        else
        {
            await _next(context);
        }
    }

    private bool IsLoginEndpoint(PathString path)
    {
        // Check for login endpoints exactly like Node.js
        return path.StartsWithSegments("/api/auth/login") || 
               path.StartsWithSegments("/api/auth/loginGoogle");
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Use IP address as client identifier exactly like Node.js
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return ipAddress;
    }

    private void UpdateRateLimit(string clientId, DateTime now)
    {
        _attempts.AddOrUpdate(clientId, 
            new RateLimitInfo { Attempts = 1, FirstAttempt = now, IsBlocked = false },
            (key, existing) =>
            {
                existing.Attempts++;
                
                // Check if within window
                if (now - existing.FirstAttempt <= _windowSize)
                {
                    if (existing.Attempts >= _maxAttempts)
                    {
                        existing.IsBlocked = true;
                        existing.BlockedUntil = now.Add(_blockDuration);
                        _logger.LogWarning($"Client {clientId} blocked due to rate limit");
                    }
                }
                else
                {
                    // Reset window
                    existing.Attempts = 1;
                    existing.FirstAttempt = now;
                    existing.IsBlocked = false;
                }
                
                return existing;
            });
    }

    private void CleanupOldEntries(DateTime now)
    {
        var keysToRemove = _attempts
            .Where(kvp => !kvp.Value.IsBlocked && now - kvp.Value.FirstAttempt > _windowSize)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _attempts.TryRemove(key, out _);
        }
    }
}

/// <summary>
/// Rate limit information
/// </summary>
public class RateLimitInfo
{
    public int Attempts { get; set; }
    public DateTime FirstAttempt { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime BlockedUntil { get; set; }
}

/// <summary>
/// Extension method to register rate limiting middleware
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
