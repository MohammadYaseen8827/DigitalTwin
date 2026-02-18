namespace DigitalTwinPlatform.API.Extensions;

/// <summary>
/// Extension methods for configuration handling and validation.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Gets a required configuration value or throws an exception.
    /// </summary>
    public static string GetRequiredValue(this IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Configuration value '{key}' is required but not configured.");
        }
        return value;
    }

    /// <summary>
    /// Gets a required connection string or throws an exception.
    /// </summary>
    public static string GetRequiredConnectionString(this IConfiguration configuration, string name)
    {
        var connectionString = configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{name}' is required but not configured.");
        }
        return connectionString;
    }

    /// <summary>
    /// Validates that required configuration sections exist.
    /// </summary>
    public static void ValidateRequiredSections(this IConfiguration configuration, params string[] sectionNames)
    {
        foreach (var sectionName in sectionNames)
        {
            var section = configuration.GetSection(sectionName);
            if (!section.Exists())
            {
                throw new InvalidOperationException($"Required configuration section '{sectionName}' is missing.");
            }
        }
    }

    /// <summary>
    /// Validates JWT configuration.
    /// </summary>
    public static void ValidateJwtConfiguration(this IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("JWT Key is not configured. Please set Jwt:Key in configuration or environment variables.");
        }

        if (jwtKey.Length < 32)
        {
            throw new InvalidOperationException("JWT Key must be at least 32 characters long for security.");
        }

        // Validate issuer and audience
        if (string.IsNullOrWhiteSpace(configuration["Jwt:Issuer"]))
        {
            throw new InvalidOperationException("JWT Issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(configuration["Jwt:Audience"]))
        {
            throw new InvalidOperationException("JWT Audience is not configured.");
        }
    }

    /// <summary>
    /// Gets configuration with fallback values.
    /// </summary>
    public static T GetValueOrDefault<T>(this IConfiguration configuration, string key, T defaultValue)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Gets a boolean configuration value with default.
    /// </summary>
    public static bool GetBooleanOrDefault(this IConfiguration configuration, string key, bool defaultValue = false)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Gets an integer configuration value with default.
    /// </summary>
    public static int GetIntOrDefault(this IConfiguration configuration, string key, int defaultValue = 0)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return int.TryParse(value, out var result) ? result : defaultValue;
    }
}