using System.Diagnostics;

namespace DigitalTwinPlatform.API.Infrastructure;

/// <summary>
/// Configures metrics collection for the application.
/// </summary>
public static class MetricsConfiguration
{
    private static readonly ActivitySource ActivitySource = new("DigitalTwinPlatform.API");

    public static IServiceCollection AddApplicationMetrics(this IServiceCollection services)
    {
        services.AddSingleton(ActivitySource);
        return services;
    }

    public static WebApplication UseApplicationMetrics(this WebApplication app)
    {
        // Middleware for request/response metrics
        app.Use(async (context, next) =>
        {
            var activity = ActivitySource.StartActivity("HTTP Request");
            activity?.SetTag("http.method", context.Request.Method);
            activity?.SetTag("http.url", context.Request.Path);
            activity?.SetTag("http.scheme", context.Request.Scheme);

            try
            {
                await next();
                activity?.SetTag("http.status_code", context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                activity?.SetTag("http.status_code", 500);
                activity?.SetTag("exception", ex.Message);
                throw;
            }
            finally
            {
                activity?.Dispose();
            }
        });

        return app;
    }
}

/// <summary>
/// Application metrics tracker.
/// </summary>
public class ApplicationMetrics
{
    public static void RecordRequest(string method, string path, int statusCode, double durationMs)
    {
        // Metrics recording would be implemented with OpenTelemetry
        // This is a placeholder for future implementation
    }

    public static void RecordError(string errorType, string message)
    {
        // Error metrics recording would be implemented with OpenTelemetry
        // This is a placeholder for future implementation
    }

    public static void SetActiveConnections(int count)
    {
        // Active connection tracking would be implemented with OpenTelemetry
        // This is a placeholder for future implementation
    }
}
