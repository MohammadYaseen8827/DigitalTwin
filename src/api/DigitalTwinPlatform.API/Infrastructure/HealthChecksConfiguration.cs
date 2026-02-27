using Microsoft.Extensions.Diagnostics.HealthChecks;
using DigitalTwinPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalTwinPlatform.API.Infrastructure;

/// <summary>
/// Configures health checks for the application.
/// </summary>
public static class HealthChecksConfiguration
{
    public static IServiceCollection AddApplicationHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // Application health check
        healthChecksBuilder.AddCheck<ApplicationHealthCheck>(
            "application",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "application", "core" });

        // Database health check
        healthChecksBuilder.AddCheck<DatabaseHealthCheck>(
            "database",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "database", "storage" });

        // ML Model health check
        healthChecksBuilder.AddCheck<MlModelsHealthCheck>(
            "ml-models",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "ml", "ai" });

        // ML Prediction Service health check
        healthChecksBuilder.AddCheck<MlPredictionHealthCheck>(
            "ml-prediction",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "ml", "prediction" });

        // External Systems health check
        healthChecksBuilder.AddCheck<ExternalSystemsHealthCheck>(
            "external-systems",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "external", "integration" });

        // Telemetry ingestion health check
        healthChecksBuilder.AddCheck<TelemetryHealthCheck>(
            "telemetry",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "telemetry", "real-time" });

        return services;
    }

    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        // Comprehensive health check with all details
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponse
        });

        // Liveness probe (is the app running?)
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("core") || check.Tags.Contains("application"),
            ResponseWriter = WriteLivenessResponse
        });

        // Readiness probe (is the app ready to accept traffic?)
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => !check.Tags.Contains("external") && !check.Tags.Contains("ml"),
            ResponseWriter = WriteReadinessResponse
        });

        // ML service specific health check
        app.MapHealthChecks("/health/ml", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ml") || check.Tags.Contains("prediction"),
            ResponseWriter = WriteHealthCheckResponse
        });

        return app;
    }

    private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds,
                data = entry.Value.Data,
                exception = entry.Value.Exception?.Message
            })
        };

        await context.Response.WriteAsJsonAsync(response);
    }

    private static async Task WriteLivenessResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var isAlive = report.Status == HealthStatus.Healthy;

        await context.Response.WriteAsJsonAsync(new { alive = isAlive, status = report.Status.ToString() });
    }

    private static async Task WriteReadinessResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var isReady = report.Status == HealthStatus.Healthy;

        await context.Response.WriteAsJsonAsync(new { ready = isReady, status = report.Status.ToString() });
    }
}

/// <summary>
/// Base application health check.
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
            return Task.FromResult(HealthCheckResult.Healthy("Application is running"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Application health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Application health check failed", ex));
        }
    }
}

/// <summary>
/// Database connectivity health check.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly DigitalTwinDbContext _dbContext;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(DigitalTwinDbContext dbContext, ILogger<DatabaseHealthCheck> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Running database health check");

            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Cannot connect to database");
            }

            var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingCount = pendingMigrations.Count();

            var data = new Dictionary<string, object>
            {
                { "pendingMigrations", pendingCount }
            };

            if (pendingCount > 0)
            {
                return HealthCheckResult.Degraded("Database has pending migrations", data: data);
            }

            return HealthCheckResult.Healthy("Database connection is healthy", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database health check failed", ex);
        }
    }
}

/// <summary>
/// ML Models availability health check.
/// </summary>
public class MlModelsHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MlModelsHealthCheck> _logger;

    public MlModelsHealthCheck(IConfiguration configuration, ILogger<MlModelsHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Running ML models health check");

            var modelsDirectory = _configuration["ML:ModelsDirectory"] ?? "./models";
            var rulModelPath = Path.Combine(modelsDirectory, _configuration["ML:RulModelPath"] ?? "rul.zip");
            var healthModelPath = Path.Combine(modelsDirectory, _configuration["ML:HealthModelPath"] ?? "health.zip");

            var data = new Dictionary<string, object>();
            var issues = new List<string>();

            var rulExists = File.Exists(rulModelPath);
            data["rulModelExists"] = rulExists;
            data["rulModelPath"] = rulModelPath;
            if (!rulExists) issues.Add("RUL model not found");

            var healthExists = File.Exists(healthModelPath);
            data["healthModelExists"] = healthExists;
            data["healthModelPath"] = healthModelPath;
            if (!healthExists) issues.Add("Health classification model not found");

            var modelFiles = Directory.Exists(modelsDirectory)
                ? Directory.GetFiles(modelsDirectory, "*.zip")
                : Array.Empty<string>();
            data["availableModels"] = modelFiles.Length;

            if (issues.Count > 0)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    string.Join("; ", issues),
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy("ML models are available", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ML models health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("ML models health check failed", ex));
        }
    }
}

/// <summary>
/// ML Prediction service health check.
/// </summary>
public class MlPredictionHealthCheck : IHealthCheck
{
    private readonly Application.Abstractions.Services.IRulPredictor _rulPredictor;
    private readonly Application.Abstractions.Services.IHealthClassifier _healthClassifier;
    private readonly ILogger<MlPredictionHealthCheck> _logger;

    public MlPredictionHealthCheck(
        Application.Abstractions.Services.IRulPredictor rulPredictor,
        Application.Abstractions.Services.IHealthClassifier healthClassifier,
        ILogger<MlPredictionHealthCheck> logger)
    {
        _rulPredictor = rulPredictor;
        _healthClassifier = healthClassifier;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Running ML prediction health check");

            var data = new Dictionary<string, object>
            {
                { "rulPredictorLoaded", _rulPredictor.IsModelLoaded },
                { "healthClassifierLoaded", _healthClassifier.IsModelLoaded }
            };

            if (!_rulPredictor.IsModelLoaded && !_healthClassifier.IsModelLoaded)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("ML prediction services not loaded", data: data));
            }

            if (!_rulPredictor.IsModelLoaded || !_healthClassifier.IsModelLoaded)
            {
                return Task.FromResult(HealthCheckResult.Degraded("Some ML prediction services not loaded", data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy("ML prediction services are ready", data: data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ML prediction health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("ML prediction health check failed", ex));
        }
    }
}

/// <summary>
/// External systems integration health check.
/// </summary>
public class ExternalSystemsHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExternalSystemsHealthCheck> _logger;

    public ExternalSystemsHealthCheck(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ExternalSystemsHealthCheck> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Running external systems health check");

            var baseUrl = _configuration["ExternalSystems:BaseUrl"];
            var data = new Dictionary<string, object>
            {
                { "configured", !string.IsNullOrEmpty(baseUrl) },
                { "baseUrl", baseUrl ?? "not configured" }
            };

            if (string.IsNullOrEmpty(baseUrl))
            {
                return HealthCheckResult.Degraded("External systems not configured", data: data);
            }

            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var response = await client.GetAsync($"{baseUrl}/health", cancellationToken);
                data["apiReachable"] = response.IsSuccessStatusCode;
                data["statusCode"] = (int)response.StatusCode;

                if (!response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Degraded($"External systems API returned {response.StatusCode}", data: data);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Could not reach external systems API");
                data["error"] = ex.Message;
                return HealthCheckResult.Degraded("External systems API unreachable", data: data);
            }

            return HealthCheckResult.Healthy("External systems are accessible", data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External systems health check failed");
            return HealthCheckResult.Unhealthy("External systems health check failed", ex);
        }
    }
}

/// <summary>
/// Telemetry ingestion health check.
/// </summary>
public class TelemetryHealthCheck : IHealthCheck
{
    private readonly Application.Abstractions.Repositories.ITelemetryRepository _telemetryRepository;
    private readonly ILogger<TelemetryHealthCheck> _logger;

    public TelemetryHealthCheck(
        Application.Abstractions.Repositories.ITelemetryRepository telemetryRepository,
        ILogger<TelemetryHealthCheck> logger)
    {
        _telemetryRepository = telemetryRepository;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Running telemetry health check");

            var recentData = await _telemetryRepository.GetRecentAsync(
                since: DateTime.UtcNow.AddMinutes(-5),
                limit: 1,
                ct: cancellationToken);

            var hasRecentData = recentData.Any();
            var data = new Dictionary<string, object>
            {
                { "recentDataAvailable", hasRecentData },
                { "checkedSince", DateTime.UtcNow.AddMinutes(-5) }
            };

            if (!hasRecentData)
            {
                return HealthCheckResult.Degraded("No telemetry data received in the last 5 minutes", data: data);
            }

            return HealthCheckResult.Healthy("Telemetry ingestion is active", data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Telemetry health check failed");
            return HealthCheckResult.Unhealthy("Telemetry health check failed", ex);
        }
    }
}
