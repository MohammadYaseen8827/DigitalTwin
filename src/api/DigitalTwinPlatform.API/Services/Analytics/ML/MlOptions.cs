namespace DigitalTwinPlatform.API.Services.Analytics.ML;

/// <summary>
/// Configuration options for ML.NET services including model paths, prediction thresholds,
/// and feature selection settings.
/// </summary>
public class MlOptions
{
    /// <summary>
    /// Path to the RUL prediction model file (.zip).
    /// </summary>
    public string? RulModelPath { get; set; }

    /// <summary>
    /// Path to the health classification model file (.zip).
    /// </summary>
    public string? HealthModelPath { get; set; }

    /// <summary>
    /// Directory for storing trained models.
    /// </summary>
    public string ModelsDirectory { get; set; } = "./models";

    /// <summary>
    /// Whether to use embedded resources as fallback models.
    /// </summary>
    public bool UseEmbeddedModels { get; set; } = true;

    /// <summary>
    /// Default prediction confidence threshold (0.0 - 1.0).
    /// </summary>
    public double ConfidenceThreshold { get; set; } = 0.7;

    /// <summary>
    /// High risk temperature threshold (°C).
    /// </summary>
    public double HighRiskTemperatureThreshold { get; set; } = 80.0;

    /// <summary>
    /// High risk vibration RMS threshold.
    /// </summary>
    public double HighRiskVibrationThreshold { get; set; } = 5.0;

    /// <summary>
    /// Number of recent telemetry points to use for predictions.
    /// </summary>
    public int TelemetryPointsForPrediction { get; set; } = 100;

    /// <summary>
    /// Minimum data points required for prediction.
    /// </summary>
    public int MinimumDataPoints { get; set; } = 20;

    /// <summary>
    /// Default RUL unit for predictions (hours, days, cycles).
    /// </summary>
    public string DefaultRulUnit { get; set; } = "hours";

    /// <summary>
    /// Feature names to use for RUL prediction.
    /// </summary>
    public List<string> EnabledFeatures { get; set; } = new()
    {
        "data_points",
        "temp_mean",
        "vib_mean",
        "vib_rms",
        "temp_std",
        "vib_std",
        "temp_trend",
        "vib_trend",
        "pressure_mean",
        "time_span_hours"
    };

    /// <summary>
    /// Feature weights for contributing factor calculation.
    /// </summary>
    public Dictionary<string, double> FeatureWeights { get; set; } = new()
    {
        ["temperature"] = 0.25,
        ["vibration"] = 0.35,
        ["pressure"] = 0.15,
        ["time"] = 0.25
    };

    /// <summary>
    /// Enable real-time prediction caching.
    /// </summary>
    public bool EnablePredictionCache { get; set; } = true;

    /// <summary>
    /// Cache TTL in seconds.
    /// </summary>
    public int CacheTtlSeconds { get; set; } = 300;

    /// <summary>
    /// Enable uncertainty quantification for predictions.
    /// </summary>
    public bool EnableUncertaintyQuantification { get; set; } = true;

    /// <summary>
    /// Hyperparameter tuning enabled.
    /// </summary>
    public bool EnableHyperparameterTuning { get; set; } = false;

    /// <summary>
    /// Cross-validation folds for model evaluation.
    /// </summary>
    public int CrossValidationFolds { get; set; } = 5;
}
