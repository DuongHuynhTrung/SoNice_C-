using Microsoft.IdentityModel.Tokens;
using SoNice.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SoNice.Api.Middleware;

/// <summary>
/// JWT middleware - matches Node.js validateTokenHandler logic exactly
/// </summary>
public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtMiddleware> _logger;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<JwtMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
        var token = ExtractTokenFromRequest(context);

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == "userId").Value;
                var userRole = jwtToken.Claims.First(x => x.Type == "role").Value;

                // Get user from database exactly like Node.js
                var userResult = await userService.GetUserByIdAsync(userId, userId, userRole);
                if (userResult.Success && userResult.Data != null)
                {
                    var user = userResult.Data;
                    
                    // Attach user to context exactly like Node.js
                    context.Items["User"] = user;
                    context.Items["UserId"] = userId;
                    context.Items["UserRole"] = userRole;
                }
                else
                {
                    _logger.LogWarning($"User {userId} not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating JWT token");
            }
        }

        await _next(context);
    }

    private string? ExtractTokenFromRequest(HttpContext context)
    {
        // Extract token from Authorization header exactly like Node.js
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        // Also check cookies exactly like Node.js
        if (context.Request.Cookies.TryGetValue("token", out var cookieToken))
        {
            return cookieToken;
        }

        return null;
    }
}

/// <summary>
/// Extension method to register JWT middleware
/// </summary>
public static class JwtMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtMiddleware>();
    }
}