using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DigitalTwinPlatform.API.Infrastructure;

/// <summary>
/// Configures health checks for the application.
/// </summary>
public static class HealthChecksConfiguration
{
    public static IServiceCollection AddApplicationHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // Custom application health check
        healthChecksBuilder.AddCheck<ApplicationHealthCheck>(
            "Application",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "application" });

        return services;
    }

    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        // Detailed health check endpoint
        app.MapHealthChecks("/health");

        // Liveness probe (is the app running?)
        app.MapHealthChecks("/health/live");

        // Readiness probe (is the app ready to accept traffic?)
        app.MapHealthChecks("/health/ready");

        return app;
    }
}

/// <summary>
/// Custom application health check.
/// </summary>
public class ApplicationHealthCheck : IHealthCheck
{
    private readonly ILogger<ApplicationHealthCheck> _logger;

    public ApplicationHealthCheck(ILogger<ApplicationHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Running application health check");
            
            // Perform any custom health checks here
            // For now, just verify the application is running
            var isHealthy = true;

            if (isHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Application is healthy"));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Application is not healthy"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Application health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Application health check failed", ex));
        }
    }
}
