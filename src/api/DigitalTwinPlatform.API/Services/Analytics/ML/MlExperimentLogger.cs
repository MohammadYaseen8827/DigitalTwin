using System.Text.Json;
using DigitalTwinPlatform.Application.Abstractions.Analytics;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.API.Services.Analytics.ML;

public class MlExperimentLogger : IMlExperimentLogger
{
    private readonly ILogger<MlExperimentLogger> _logger;
    private readonly string _logPath = "ml_experiments.json";

    public MlExperimentLogger(ILogger<MlExperimentLogger> logger)
    {
        _logger = logger;
    }

    public async Task LogExperimentAsync(string modelType, string version, double rSquared, double mape, string metadata = "")
    {
        var experiment = new
        {
            Timestamp = DateTime.UtcNow,
            ModelType = modelType,
            Version = version,
            Metrics = new { RSquared = rSquared, MAPE = mape },
            Metadata = metadata
        };

        _logger.LogInformation("Logging experiment: {ModelType} v{Version}, MAPE: {MAPE:P2}", modelType, version, mape);

        try
        {
            var logs = new List<object>();
            if (File.Exists(_logPath))
            {
                var existingJson = await File.ReadAllTextAsync(_logPath);
                logs = JsonSerializer.Deserialize<List<object>>(existingJson) ?? new List<object>();
            }

            logs.Add(experiment);
            await File.WriteAllTextAsync(_logPath, JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write experiment log to disk");
        }
    }
}
