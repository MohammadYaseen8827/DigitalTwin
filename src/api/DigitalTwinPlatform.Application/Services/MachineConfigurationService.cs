using System.Text.Json;
using DigitalTwinPlatform.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DigitalTwinPlatform.Application.Services;

public class ConfigurationOptions
{
    public string ConfigDirectory { get; set; } = "config/machines";
}

public class MachineConfigurationService : IMachineConfigurationService
{
    private readonly IOptions<ConfigurationOptions> _options;
    private readonly ILogger<MachineConfigurationService> _logger;
    private readonly string _configDirectory;

    public MachineConfigurationService(
        IOptions<ConfigurationOptions> options,
        ILogger<MachineConfigurationService> logger)
    {
        _options = options;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Determine config directory path
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var configPath = _options.Value.ConfigDirectory;
        
        // Handle relative paths
        if (Path.IsPathRooted(configPath))
        {
            _configDirectory = configPath;
        }
        else
        {
            // Try to find config directory relative to solution root
            var currentDir = Directory.GetCurrentDirectory();
            var solutionRoot = FindSolutionRoot(currentDir);
            _configDirectory = Path.Combine(solutionRoot ?? currentDir, configPath);
        }
        
        // Ensure directory exists
        Directory.CreateDirectory(_configDirectory);
        _logger.LogInformation("Machine configuration directory: {ConfigDirectory}", _configDirectory);
    }

    private static string? FindSolutionRoot(string startPath)
    {
        var directory = new DirectoryInfo(startPath);
        while (directory != null)
        {
            if (directory.GetFiles("*.sln").Length > 0)
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        return null;
    }

    public async Task<MachineConfiguration> LoadConfigurationAsync(
        string machineType, 
        CancellationToken ct = default)
    {
        var configPath = Path.Combine(_configDirectory, $"{machineType}.json");
        
        if (!File.Exists(configPath))
        {
            _logger.LogWarning("Configuration not found for machine type: {MachineType} at path: {Path}", 
                machineType, configPath);
            throw new FileNotFoundException($"Configuration not found for machine type: {machineType}");
        }
        
        var jsonContent = await File.ReadAllTextAsync(configPath, ct);
        
        try
        {
            var config = JsonSerializer.Deserialize<MachineConfiguration>(
                jsonContent,
                new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }
            );
            
            if (config == null)
            {
                throw new InvalidOperationException($"Failed to deserialize configuration for: {machineType}");
            }
            
            _logger.LogInformation("Successfully loaded configuration for machine type: {MachineType}", machineType);
            return config;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON configuration for machine type: {MachineType}", machineType);
            throw new InvalidOperationException($"Invalid JSON configuration for machine type: {machineType}", ex);
        }
    }

    public async Task<List<MachineConfiguration>> LoadAllConfigurationsAsync(CancellationToken ct = default)
    {
        var configurations = new List<MachineConfiguration>();
        
        if (!Directory.Exists(_configDirectory))
        {
            _logger.LogWarning("Configuration directory does not exist: {ConfigDirectory}", _configDirectory);
            return configurations;
        }
        
        var configFiles = Directory.GetFiles(_configDirectory, "*.json");
        
        foreach (var configFile in configFiles)
        {
            try
            {
                var machineType = Path.GetFileNameWithoutExtension(configFile);
                var config = await LoadConfigurationAsync(machineType, ct);
                configurations.Add(config);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load configuration from file: {ConfigFile}", configFile);
                // Continue loading other configurations
            }
        }
        
        return configurations;
    }

    public Task<bool> ValidateConfigurationAsync(string jsonContent, CancellationToken ct = default)
    {
        try
        {
            var config = JsonSerializer.Deserialize<MachineConfiguration>(
                jsonContent,
                new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }
            );
            
            if (config == null)
            {
                return Task.FromResult(false);
            }
            
            // Basic validation
            if (string.IsNullOrWhiteSpace(config.MachineType))
            {
                return Task.FromResult(false);
            }
            
            if (config.DegradationModel == null)
            {
                return Task.FromResult(false);
            }
            
            if (config.SensorMappings == null || config.SensorMappings.Count == 0)
            {
                return Task.FromResult(false);
            }
            
            if (config.FailureThresholds == null)
            {
                return Task.FromResult(false);
            }
            
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            // Log at Information level when validation fails, so support can see that fallback was used
            _logger.LogInformation(ex, "Configuration validation failed due to parsing error");
            return Task.FromResult(false);
        }
    }

    public async Task SaveConfigurationAsync(string machineType, MachineConfiguration config, CancellationToken ct = default)
    {
        var configPath = Path.Combine(_configDirectory, $"{machineType}.json");
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var jsonContent = JsonSerializer.Serialize(config, options);
        await File.WriteAllTextAsync(configPath, jsonContent, ct);
        
        _logger.LogInformation("Saved configuration for machine type: {MachineType} to {Path}", machineType, configPath);
    }

    public Task<bool> ConfigurationExistsAsync(string machineType, CancellationToken ct = default)
    {
        var configPath = Path.Combine(_configDirectory, $"{machineType}.json");
        return Task.FromResult(File.Exists(configPath));
    }
}
