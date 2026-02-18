using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Enums;
using Microsoft.ML;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using DigitalTwinPlatform.Application.Abstractions.Services;
using DigitalTwinPlatform.Domain.Entities.Enums;

namespace DigitalTwinPlatform.API.Services.Analytics.ML;

/// <summary>
/// Service for classifying machine health status using ML.NET.
/// Supports real-time classification, confidence scores, and contributing factor analysis.
/// </summary>
public class HealthClassifier : IHealthClassifier, IDisposable
{
    private readonly PredictionEngine<HealthModelInput, HealthModelOutput>? _engine;
    private readonly MLContext _mlContext;
    private readonly MlOptions _options;
    private readonly ILogger<HealthClassifier> _logger;
    private bool _disposed;

    /// <inheritdoc />
    public bool IsModelLoaded => _engine != null;

    /// <summary>
    /// Initializes a new instance of the HealthClassifier.
    /// </summary>
    public HealthClassifier(IOptions<MlOptions> options, ILogger<HealthClassifier> logger)
    {
        _options = options.Value;
        _logger = logger;
        _mlContext = new MLContext(seed: 42);

        try
        {
            _engine = LoadPredictionEngine();
            if (_engine != null)
            {
                _logger.LogInformation("Health classification engine initialized successfully");
            }
            else
            {
                _logger.LogWarning("Health classifier using fallback mode - no model loaded");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize health classification engine");
            _engine = null;
        }
    }

    /// <inheritdoc />
    public HealthClassificationResult ClassifyWithDetails(string machineId, Dictionary<string, double> features)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var (probability, health) = ClassifyWithFeatures(features);
            var contributingFactors = GetContributingFactors(features);

            stopwatch.Stop();
            _logger.LogInformation(
                "Health classification for machine {MachineId}: {Health} (confidence: {Confidence:P2}, time: {Time}ms)",
                machineId, health, probability, stopwatch.ElapsedMilliseconds);

            return new HealthClassificationResult
            {
                MachineId = machineId,
                HealthStatus = health,
                HealthProbability = Math.Round(probability, 3),
                Confidence = Math.Round(CalculateConfidence(probability), 2),
                ContributingFactors = contributingFactors,
                ClassificationTime = DateTime.UtcNow,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                Recommendation = GetRecommendation(health, probability)
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Health classification failed for machine {MachineId}", machineId);

            var fallback = CalculateFallbackHealth(features);
            return new HealthClassificationResult
            {
                MachineId = machineId,
                HealthStatus = fallback.health,
                HealthProbability = Math.Round(fallback.probability, 3),
                Confidence = 0.5,
                ContributingFactors = GetContributingFactors(features),
                ClassificationTime = DateTime.UtcNow,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                Recommendation = GetRecommendation(fallback.health, fallback.probability),
                IsFallback = true
            };
        }
    }

    /// <summary>
    /// Classifies health status from telemetry data.
    /// </summary>
    public (double probability, HealthClassification health) Classify(IEnumerable<TelemetryData> telemetry)
    {
        var features = ExtractFeaturesFromTelemetry(telemetry);
        return ClassifyWithFeatures(features);
    }

    /// <inheritdoc />
    public (double probability, HealthClassification health) ClassifyWithFeatures(Dictionary<string, double> features)
    {
        if (_engine == null)
        {
            return CalculateFallbackHealth(features);
        }

        try
        {
            var input = CreateModelInput(features);
            var result = _engine.Predict(input);
            return (result.Probability, result.Health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health classification prediction failed, using fallback");
            return CalculateFallbackHealth(features);
        }
    }

    /// <inheritdoc />
    public List<HealthContributingFactor> GetContributingFactors(Dictionary<string, double> features)
    {
        var factors = new List<HealthContributingFactor>();

        if (features.TryGetValue("temp_mean", out var tempMean))
        {
            factors.Add(new HealthContributingFactor
            {
                Feature = "temperature",
                Value = Math.Round(tempMean, 1),
                Impact = CalculateTemperatureImpact(tempMean),
                Threshold = _options.HighRiskTemperatureThreshold,
                IsWithinThreshold = tempMean <= _options.HighRiskTemperatureThreshold
            });
        }

        if (features.TryGetValue("vib_rms", out var vibRms))
        {
            factors.Add(new HealthContributingFactor
            {
                Feature = "vibration",
                Value = Math.Round(vibRms, 2),
                Impact = CalculateVibrationImpact(vibRms),
                Threshold = _options.HighRiskVibrationThreshold,
                IsWithinThreshold = vibRms <= _options.HighRiskVibrationThreshold
            });
        }

        if (features.TryGetValue("temp_trend", out var tempTrend))
        {
            factors.Add(new HealthContributingFactor
            {
                Feature = "temperature_trend",
                Value = Math.Round(tempTrend, 4),
                Impact = Math.Abs(tempTrend) > 0.1 ? 0.2 : 0.0,
                Threshold = 0.1,
                IsWithinThreshold = Math.Abs(tempTrend) <= 0.1
            });
        }

        if (features.TryGetValue("vib_trend", out var vibTrend))
        {
            factors.Add(new HealthContributingFactor
            {
                Feature = "vibration_trend",
                Value = Math.Round(vibTrend, 4),
                Impact = Math.Abs(vibTrend) > 0.1 ? 0.25 : 0.0,
                Threshold = 0.1,
                IsWithinThreshold = Math.Abs(vibTrend) <= 0.1
            });
        }

        if (features.TryGetValue("temp_std", out var tempStd))
        {
            factors.Add(new HealthContributingFactor
            {
                Feature = "temperature_variability",
                Value = Math.Round(tempStd, 2),
                Impact = tempStd > 5.0 ? 0.15 : 0.0,
                Threshold = 5.0,
                IsWithinThreshold = tempStd <= 5.0
            });
        }

        if (features.TryGetValue("vib_std", out var vibStd))
        {
            factors.Add(new HealthContributingFactor
            {
                Feature = "vibration_variability",
                Value = Math.Round(vibStd, 2),
                Impact = vibStd > 2.0 ? 0.15 : 0.0,
                Threshold = 2.0,
                IsWithinThreshold = vibStd <= 2.0
            });
        }

        return factors.OrderByDescending(f => f.Impact).ToList();
    }

    /// <inheritdoc />
    public double GetRiskScore(Dictionary<string, double> features)
    {
        var (probability, _) = ClassifyWithFeatures(features);
        return probability;
    }

    private PredictionEngine<HealthModelInput, HealthModelOutput>? LoadPredictionEngine()
    {
        var modelPath = _options.HealthModelPath;
        if (!string.IsNullOrWhiteSpace(modelPath) && File.Exists(modelPath))
        {
            try
            {
                var model = _mlContext.Model.Load(modelPath, out var schema);
                return _mlContext.Model.CreatePredictionEngine<HealthModelInput, HealthModelOutput>(model);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load health model from {Path}", modelPath);
            }
        }

        if (_options.UseEmbeddedModels)
        {
            return TryLoadEmbeddedFallback();
        }

        return null;
    }

    private PredictionEngine<HealthModelInput, HealthModelOutput>? TryLoadEmbeddedFallback()
    {
        var assembly = typeof(HealthClassifier).Assembly;
        var resourceNames = assembly.GetManifestResourceNames();
        var healthResource = resourceNames.FirstOrDefault(n => n.Contains("HealthModel", StringComparison.OrdinalIgnoreCase));
        if (healthResource != null)
        {
            try
            {
                using var stream = assembly.GetManifestResourceStream(healthResource);
                if (stream != null)
                {
                    var model = _mlContext.Model.Load(stream, out var schema);
                    _logger.LogInformation("Loaded embedded health model from {ResourceName}", healthResource);
                    return _mlContext.Model.CreatePredictionEngine<HealthModelInput, HealthModelOutput>(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load embedded health model");
            }
        }
        return null;
    }

    private HealthModelInput CreateModelInput(Dictionary<string, double> features)
    {
        return new HealthModelInput
        {
            Feature1 = features.TryGetValue("data_points", out var dataPoints) ? (float)dataPoints : 0f,
            Feature2 = features.TryGetValue("temp_mean", out var tempMean) ? (float)tempMean : 0f,
            Feature3 = features.TryGetValue("vib_mean", out var vibMean) ? (float)vibMean : 0f,
            Feature4 = features.TryGetValue("vib_rms", out var vibRms) ? (float)vibRms : 0f,
            Feature5 = features.TryGetValue("temp_std", out var tempStd) ? (float)tempStd : 0f,
            Feature6 = features.TryGetValue("vib_std", out var vibStd) ? (float)vibStd : 0f,
            Feature7 = features.TryGetValue("temp_trend", out var tempTrend) ? (float)tempTrend : 0f,
            Feature8 = features.TryGetValue("vib_trend", out var vibTrend) ? (float)vibTrend : 0f,
            Feature9 = features.TryGetValue("pressure_mean", out var pressureMean) ? (float)pressureMean : 0f,
            Feature10 = features.TryGetValue("time_span_hours", out var timeSpan) ? (float)timeSpan : 0f
        };
    }

    private static double CalculateTemperatureImpact(double tempMean)
    {
        if (tempMean > 90) return 0.4;
        if (tempMean > 80) return 0.25;
        if (tempMean > 70) return 0.1;
        return 0.0;
    }

    private static double CalculateVibrationImpact(double vibRms)
    {
        if (vibRms > 10) return 0.5;
        if (vibRms > 7) return 0.3;
        if (vibRms > 5) return 0.15;
        return 0.0;
    }

    private static (double probability, HealthClassification health) CalculateFallbackHealth(Dictionary<string, double> features)
    {
        var riskScore = 0.0;
        var healthFactors = 0;

        if (features.TryGetValue("temp_mean", out var tempMean))
        {
            healthFactors++;
            if (tempMean > 90) riskScore += 0.4;
            else if (tempMean > 80) riskScore += 0.2;
            else if (tempMean > 70) riskScore += 0.1;
        }

        if (features.TryGetValue("vib_rms", out var vibRms))
        {
            healthFactors++;
            if (vibRms > 10) riskScore += 0.5;
            else if (vibRms > 7) riskScore += 0.3;
            else if (vibRms > 5) riskScore += 0.15;
        }

        if (features.TryGetValue("temp_trend", out var tempTrend))
        {
            healthFactors++;
            if (tempTrend > 0.2) riskScore += 0.3;
            else if (tempTrend > 0.1) riskScore += 0.15;
        }

        if (features.TryGetValue("vib_trend", out var vibTrend))
        {
            healthFactors++;
            if (vibTrend > 0.2) riskScore += 0.3;
            else if (vibTrend > 0.1) riskScore += 0.15;
        }

        if (features.TryGetValue("temp_std", out var tempStd) && tempStd > 5)
        {
            riskScore += 0.1;
        }

        if (features.TryGetValue("vib_std", out var vibStd) && vibStd > 2)
        {
            riskScore += 0.1;
        }

        var normalizedRisk = healthFactors > 0 ? riskScore / healthFactors : 0.0;
        var probability = Math.Min(1.0, Math.Max(0.0, normalizedRisk));

        var health = normalizedRisk switch
        {
            >= 0.7 => HealthClassification.FailureImminent,
            >= 0.5 => HealthClassification.SignificantDegradation,
            >= 0.3 => HealthClassification.MinorDegradation,
            >= 0.1 => HealthClassification.Normal,
            _ => HealthClassification.Healthy
        };

        return (probability, health);
    }

    private static double CalculateConfidence(double probability)
    {
        return Math.Abs(probability - 0.5) * 2;
    }

    private static string GetRecommendation(HealthClassification health, double probability)
    {
        return health switch
        {
            HealthClassification.FailureImminent => "URGENT: Schedule immediate maintenance. Machine failure expected soon.",
            HealthClassification.SignificantDegradation => "Schedule maintenance within 24-48 hours. Critical components degrading.",
            HealthClassification.MinorDegradation => "Plan maintenance within the next week. Minor issues detected.",
            HealthClassification.Normal => "Machine operating normally. Continue regular monitoring.",
            HealthClassification.Healthy => "Machine in excellent condition. No action required.",
            _ => "Monitor machine closely for changes in behavior."
        };
    }

    private static Dictionary<string, double> ExtractFeaturesFromTelemetry(IEnumerable<TelemetryData> telemetry)
    {
        var telemetryList = telemetry.ToList();
        var features = new Dictionary<string, double>();

        var tempData = telemetryList.Where(t => t.DataType == "temperature").ToList();
        if (tempData.Count > 0)
        {
            var temps = tempData.Select(t => t.Data.RootElement.TryGetProperty("value", out var v) && v.TryGetDouble(out var val) ? val : 0).ToList();
            features["temp_mean"] = temps.Average();
            features["temp_std"] = CalculateStdDev(temps);
            features["temp_trend"] = CalculateTrend(temps);
        }

        var vibData = telemetryList.Where(t => t.DataType == "vibration").ToList();
        if (vibData.Count > 0)
        {
            var vibes = vibData.Select(t => t.Data.RootElement.TryGetProperty("amplitude", out var v) && v.TryGetDouble(out var val) ? val : 0).ToList();
            features["vib_mean"] = vibes.Average();
            features["vib_std"] = CalculateStdDev(vibes);
            features["vib_rms"] = Math.Sqrt(vibes.Average(v => v * v));
            features["vib_trend"] = CalculateTrend(vibes);
        }

        var pressureData = telemetryList.Where(t => t.DataType == "pressure").ToList();
        if (pressureData.Count > 0)
        {
            var pressures = pressureData.Select(t => t.Data.RootElement.TryGetProperty("value", out var v) && v.TryGetDouble(out var val) ? val : 0).ToList();
            features["pressure_mean"] = pressures.Average();
        }

        features["data_points"] = telemetryList.Count;
        features["time_span_hours"] = telemetryList.Count > 0
            ? (telemetryList.Max(t => t.Timestamp) - telemetryList.Min(t => t.Timestamp)).TotalHours
            : 0;

        return features;
    }

    private static double CalculateStdDev(List<double> values)
    {
        if (values.Count < 2) return 0;
        var mean = values.Average();
        return Math.Sqrt(values.Average(v => Math.Pow(v - mean, 2)));
    }

    private static double CalculateTrend(List<double> values)
    {
        if (values.Count < 2) return 0;
        var n = values.Count;
        var x = Enumerable.Range(0, n).Select(i => (double)i).ToArray();
        var y = values.ToArray();
        var xMean = x.Average();
        var yMean = y.Average();
        var numerator = x.Zip(y, (xi, yi) => (xi - xMean) * (yi - yMean)).Sum();
        var denominator = x.Sum(xi => Math.Pow(xi - xMean, 2));
        return denominator == 0 ? 0 : numerator / denominator;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _engine?.Dispose();
            _disposed = true;
        }
    }
}

public class HealthModelInput
{
    public float Feature1 { get; set; }
    public float Feature2 { get; set; }
    public float Feature3 { get; set; }
    public float Feature4 { get; set; }
    public float Feature5 { get; set; }
    public float Feature6 { get; set; }
    public float Feature7 { get; set; }
    public float Feature8 { get; set; }
    public float Feature9 { get; set; }
    public float Feature10 { get; set; }
}

public class HealthModelOutput
{
    public float Probability { get; set; }
    public HealthClassification Health { get; set; }
}
