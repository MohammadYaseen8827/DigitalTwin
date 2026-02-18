// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Logging;
//
// namespace DigitalTwinPlatform.Application.Configuration;
//
// /// <summary>
// /// Service for managing environment-specific configurations
// /// </summary>
// public interface IEnvironmentConfigurationService
// {
//     string GetCurrentEnvironment();
//     bool IsDevelopment();
//     bool IsStaging();
//     bool IsProduction();
//     T GetEnvironmentValue<T>(string key, T defaultValue = default);
//     bool IsFeatureEnabled(string featureName);
//     string GetConnectionString();
//     string GetJwtConfiguration();
// }
//
// public class EnvironmentConfigurationService : IEnvironmentConfigurationService
// {
//     private readonly IConfiguration _configuration;
//     private readonly ILogger<EnvironmentConfigurationService> _logger;
//     private readonly string _environment;
//
//     public EnvironmentConfigurationService(IConfiguration configuration, ILogger<EnvironmentConfigurationService> logger)
//     {
//         _configuration = configuration;
//         _logger = logger;
//         _environment = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") ?? "Development";
//         _logger.LogInformation("Environment Configuration Service initialized for environment: {Environment}", _environment);
//     }
//     
//
//     public string GetCurrentEnvironment()
//     {
//         return _environment;
//     }
//
//     public bool IsDevelopment()
//     {
//         return _environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
//     }
//
//     public bool IsStaging()
//     {
//         return _environment.Equals("Staging", StringComparison.OrdinalIgnoreCase);
//     }
//
//     public bool IsProduction()
//     {
//         return _environment.Equals("Production", StringComparison.OrdinalIgnoreCase);
//     }
//
//     public T GetEnvironmentValue<T>(string key, T defaultValue = default)
//     {
//         var value = _configuration[key];
//         if (value == null)
//         {
//             _logger.LogDebug("Environment value not found for key: {Key}, using default value", key);
//             return defaultValue;
//         }
//
//         try
//         {
//             return _configuration.GetValue<T>(key);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error converting environment value for key: {Key}", key);
//             return defaultValue;
//         }
//     }
//
//     public bool IsFeatureEnabled(string featureName)
//     {
//         var featurePath = $"FeatureFlags:{featureName}";
//         var isEnabled = GetEnvironmentValue(featurePath, false);
//         
//         _logger.LogDebug("Feature {FeatureName} is {Enabled}", featureName, isEnabled ? "enabled" : "disabled");
//         return isEnabled;
//     }
//
//     public string GetConnectionString()
//     {
//         var connectionString = GetEnvironmentValue<string>("ConnectionStrings:DefaultConnection");
//         
//         if (string.IsNullOrEmpty(connectionString))
//         {
//             throw new InvalidOperationException("Database connection string not found in configuration");
//         }
//
//         // Log connection string type for security audit
//         if (IsDevelopment())
//         {
//             _logger.LogDebug("Using development database connection");
//         }
//         else
//         {
//             _logger.LogInformation("Using production database connection");
//         }
//
//         return connectionString;
//     }
//
//     public string GetJwtConfiguration()
//     {
//         var key = GetEnvironmentValue<string>("Jwt:Key");
//         var issuer = GetEnvironmentValue<string>("Jwt:Issuer");
//         var audience = GetEnvironmentValue<string>("Jwt:Audience");
//         var expirationMinutes = GetEnvironmentValue<object>("Jwt:ExpirationMinutes");
//
//         if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
//         {
//             throw new InvalidOperationException("JWT configuration is incomplete");
//         }
//
//         if (IsDevelopment() && key.Contains("dev-secret"))
//         {
//             _logger.LogWarning("Development JWT secret detected - ensure this is changed in production");
//         }
//
//         _logger.LogInformation("JWT configuration loaded for issuer: {Issuer}", issuer);
//         return $"Issuer: {issuer}, Audience: {audience}, Expiration: {expirationMinutes} minutes";
//     }
//
//     /// <summary>
//     /// Gets environment-specific performance settings
//     /// </summary>
//     public PerformanceSettings GetPerformanceSettings()
//     {
//         return new PerformanceSettings
//         {
//             EnableResponseCaching = GetEnvironmentValue("Performance:EnableResponseCaching", IsProduction()),
//             CacheDurationMinutes = GetEnvironmentValue("Performance:CacheDurationMinutes", IsProduction() ? 15 : 5),
//             MaxConcurrentRequests = GetEnvironmentValue("Performance:MaxConcurrentRequests", IsProduction() ? 5000 : 100),
//             RequestTimeoutSeconds = GetEnvironmentValue("Performance:RequestTimeoutSeconds", IsProduction() ? 20 : 60)
//         };
//     }
//
//     /// <summary>
//     /// Gets environment-specific security settings
//     /// </summary>
//     public SecuritySettings GetSecuritySettings()
//     {
//         return new SecuritySettings
//         {
//             RequireHttps = GetEnvironmentValue("Security:RequireHttps", IsProduction()),
//             EnableRateLimiting = GetEnvironmentValue("Security:EnableRateLimiting", IsProduction()),
//             RateLimitPerMinute = GetEnvironmentValue("Security:RateLimitPerMinute", IsProduction() ? 500 : 10000),
//             EnableCsp = GetEnvironmentValue("Security:EnableCsp", IsProduction()),
//             EnableHsts = GetEnvironmentValue("Security:EnableHsts", IsProduction())
//         };
//     }
//
//     /// <summary>
//     /// Gets environment-specific ML settings
//     /// </summary>
//     public MLSettings GetMLSettings()
//     {
//         return new MLSettings
//         {
//             ModelRetrainingIntervalHours = GetEnvironmentValue("ML:ModelRetrainingIntervalHours", IsProduction() ? 24 : 1),
//             MinTrainingSamples = GetEnvironmentValue("ML:MinTrainingSamples", IsProduction() ? 100 : 10),
//             MaxTrainingSamples = GetEnvironmentValue("ML:MaxTrainingSamples", IsProduction() ? 50000 : 1000),
//             EnableModelVersioning = GetEnvironmentValue("ML:EnableModelVersioning", true),
//             ModelStoragePath = GetEnvironmentValue("ML:ModelStoragePath", IsProduction() ? "/app/models/production" : "./models/dev")
//         };
//     }
// }
//
// /// <summary>
// /// Performance configuration settings
// /// </summary>
// public class PerformanceSettings
// {
//     public bool EnableResponseCaching { get; set; }
//     public int CacheDurationMinutes { get; set; }
//     public int MaxConcurrentRequests { get; set; }
//     public int RequestTimeoutSeconds { get; set; }
// }
//
// /// <summary>
// /// Security configuration settings
// /// </summary>
// public class SecuritySettings
// {
//     public bool RequireHttps { get; set; }
//     public bool EnableRateLimiting { get; set; }
//     public int RateLimitPerMinute { get; set; }
//     public bool EnableCsp { get; set; }
//     public bool EnableHsts { get; set; }
// }
//
// /// <summary>
// /// Machine Learning configuration settings
// /// </summary>
// public class MLSettings
// {
//     public int ModelRetrainingIntervalHours { get; set; }
//     public int MinTrainingSamples { get; set; }
//     public int MaxTrainingSamples { get; set; }
//     public bool EnableModelVersioning { get; set; }
//     public string ModelStoragePath { get; set; } = "";
// }
