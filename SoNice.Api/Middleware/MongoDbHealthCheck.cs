using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace SoNice.Api.Middleware;

/// <summary>
/// MongoDB health check for ASP.NET Core health checks
/// </summary>
public class MongoDbHealthCheck : IHealthCheck
{
    private readonly IMongoDatabase _database;

    public MongoDbHealthCheck(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _database.RunCommandAsync((Command<object>)"{ping:1}", cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy("MongoDB connection is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("MongoDB connection is unhealthy", ex);
        }
    }
}

/// <summary>
/// MongoDB health check middleware - matches Node.js database connection logic
/// </summary>
public class MongoDbHealthCheckMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MongoDbHealthCheckMiddleware> _logger;
    private readonly IMongoDatabase _database;

    public MongoDbHealthCheckMiddleware(RequestDelegate next, ILogger<MongoDbHealthCheckMiddleware> logger, IMongoDatabase database)
    {
        _next = next;
        _logger = logger;
        _database = database;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check MongoDB connection health
        try
        {
            await _database.RunCommandAsync((Command<object>)"{ping:1}");
            context.Items["MongoDbHealth"] = "healthy";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB health check failed");
            context.Items["MongoDbHealth"] = "unhealthy";
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method to register MongoDB health check middleware
/// </summary>
public static class MongoDbHealthCheckMiddlewareExtensions
{
    public static IApplicationBuilder UseMongoDbHealthCheck(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MongoDbHealthCheckMiddleware>();
    }
}