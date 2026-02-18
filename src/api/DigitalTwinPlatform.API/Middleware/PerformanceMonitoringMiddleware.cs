using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace DigitalTwinPlatform.API.Middleware
{
    /// <summary>
    /// High-performance monitoring middleware for request/response metrics
    /// </summary>
    public class PerformanceMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
        private readonly PerformanceMonitoringOptions _options;

        public PerformanceMonitoringMiddleware(
            RequestDelegate next,
            ILogger<PerformanceMonitoringMiddleware> logger,
            IOptions<PerformanceMonitoringOptions> options)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip monitoring for excluded paths
            if (ShouldExcludePath(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var startTime = DateTime.UtcNow;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var duration = stopwatch.ElapsedMilliseconds;

                // Log slow requests
                if (duration > _options.SlowRequestThresholdMs)
                {
                    _logger.LogWarning(
                        "Slow request detected: {Method} {Path} took {Duration}ms. Status: {StatusCode}",
                        context.Request.Method,
                        context.Request.Path,
                        duration,
                        context.Response.StatusCode);
                }

                // Collect metrics
                CollectMetrics(context, duration, startTime);
            }
        }

        private bool ShouldExcludePath(PathString path)
        {
            return _options.ExcludedPaths.Any(excluded => 
                path.StartsWithSegments(excluded, StringComparison.OrdinalIgnoreCase));
        }

        private void CollectMetrics(HttpContext context, long duration, DateTime startTime)
        {
            // In a real implementation, you would send these to your monitoring system
            // This is a simplified example
            
            var metrics = new
            {
                Request = new
                {
                    Method = context.Request.Method,
                    Path = context.Request.Path.ToString(),
                    QueryString = context.Request.QueryString.ToString(),
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    ClientIP = context.Connection.RemoteIpAddress?.ToString(),
                    StartTime = startTime,
                    DurationMs = duration,
                    StatusCode = context.Response.StatusCode,
                    ResponseSize = context.Response.ContentLength
                },
                Performance = new
                {
                    IsSlowRequest = duration > _options.SlowRequestThresholdMs,
                    DurationBucket = GetDurationBucket(duration)
                }
            };

            // Log metrics (in production, send to monitoring system like Prometheus)
            _logger.LogInformation("Request metrics: {@Metrics}", metrics);
        }

        private string GetDurationBucket(long duration)
        {
            return duration switch
            {
                <= 50 => "0-50ms",
                <= 100 => "51-100ms",
                <= 200 => "101-200ms",
                <= 500 => "201-500ms",
                <= 1000 => "501-1000ms",
                _ => "1000ms+"
            };
        }
    }

    public class PerformanceMonitoringOptions
    {
        public int SlowRequestThresholdMs { get; set; } = 1000;
        public List<string> ExcludedPaths { get; set; } = new()
        {
            "/health",
            "/metrics",
            "/favicon.ico"
        };
    }

    // Extension method for easy registration
    public static class PerformanceMonitoringExtensions
    {
        public static IServiceCollection AddPerformanceMonitoring(
            this IServiceCollection services,
            Action<PerformanceMonitoringOptions>? configureOptions = null)
        {
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }
            else
            {
                services.Configure<PerformanceMonitoringOptions>(options => { });
            }

            return services;
        }

        public static IApplicationBuilder UsePerformanceMonitoring(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PerformanceMonitoringMiddleware>();
        }
    }
}