namespace DigitalTwinPlatform.API.Infrastructure;

/// <summary>
/// Configures structured logging configuration.
/// </summary>
public static class LoggingConfiguration
{
    public static IServiceCollection AddStructuredLogging(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure logging through standard .NET logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.AddDebug();
            
            var logLevel = configuration["Logging:LogLevel:Default"] ?? "Information";
            if (Enum.TryParse<LogLevel>(logLevel, out var level))
            {
                builder.SetMinimumLevel(level);
            }
        });

        return services;
    }

    public static WebApplication UseStructuredLogging(this WebApplication app)
    {
        // Add request logging middleware
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            var startTime = DateTime.UtcNow;
            var path = context.Request.Path;
            var method = context.Request.Method;

            logger.LogInformation("Request started: {Method} {Path}", method, path);

            try
            {
                await next();
                var duration = DateTime.UtcNow - startTime;
                logger.LogInformation("Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {DurationMs}ms", 
                    method, path, context.Response.StatusCode, duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                logger.LogError(ex, "Request failed: {Method} {Path} - Duration: {DurationMs}ms", 
                    method, path, duration.TotalMilliseconds);
                throw;
            }
        });

        return app;
    }
}
