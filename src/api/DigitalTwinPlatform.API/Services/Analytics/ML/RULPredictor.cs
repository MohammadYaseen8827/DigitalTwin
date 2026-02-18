using DigitalTwinPlatform.Domain.Entities;
using Microsoft.ML;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using DigitalTwinPlatform.Application.Abstractions.Services;

namespace DigitalTwinPlatform.API.Services.Analytics.ML;

/// <summary>
/// Service for predicting Remaining Useful Life (RUL) of machines using ML.NET.
/// Supports real-time prediction, confidence intervals, and feature contribution analysis.
/// </summary>
public class RulPredictor : IRulPredictor, IDisposable
{
    private PredictionEngine<RulModelInput, RulModelOutput>? _engine;
    private ITransformer? _model;
    private readonly MLContext _mlContext;
    private readonly MlOptions _options;
    private readonly ILogger<RulPredictor> _logger;
    private readonly Dictionary<string, double> _featureWeights;
    private bool _disposed;

    /// <inheritdoc />
    public bool IsModelLoaded => _engine != null;

    /// <summary>
    /// Initializes a new instance of RUL predictor.
    /// </summary>
    public RulPredictor(IOptions<MlOptions> options, ILogger<RulPredictor> logger)
    {
        _options = options.Value;
        _logger = logger;
        _mlContext = new MLContext(seed: 42);
        _featureWeights = InitializeFeatureWeights();

        try
        {
            _engine = LoadPredictionEngine();
            if (_engine != null)
            {
                _logger.LogInformation("RUL prediction engine initialized successfully");
            }
            else
            {
                _logger.LogWarning("RUL prediction engine using fallback mode - no model loaded");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RUL prediction engine");
        }
    }

    /// <inheritdoc />
    public RulPredictionResult PredictWithDetails(string machineId, Dictionary<string, double> features)
    {
        var stopwatch = Stopwatch.StartNew();
        var predictionTime = DateTime.UtcNow;

        try
        {
            var (prediction, lowerBound, upperBound) = PredictWithRange(features);
            var confidence = CalculateConfidence(lowerBound, upperBound, prediction);
            var contributingFactors = GetContributingFactors(features);
            var rulUnit = _options.DefaultRulUnit;
            var convertedRul = ConvertRulToUnit(prediction, rulUnit);
            var convertedLower = ConvertRulToUnit(lowerBound, rulUnit);
            var convertedUpper = ConvertRulToUnit(upperBound, rulUnit);

            stopwatch.Stop();
            _logger.LogInformation(
                "RUL prediction for machine {MachineId}: {RUL} {Unit} (confidence: {Confidence:P2}, time: {Time}ms)",
                machineId, convertedRul, rulUnit, confidence, stopwatch.ElapsedMilliseconds);

            // Calculate additional fields for frontend compatibility
            var degradationRate = CalculateDegradationRate(features);
            var trend = DetermineTrend(degradationRate);
            var estimatedFailureDate = CalculateEstimatedFailureDate(predictionTime, convertedRul, rulUnit);
            var healthScore = CalculateHealthScore(prediction, confidence);

            return new RulPredictionResult
            {
                MachineId = machineId,
                Rul = Math.Round(convertedRul, 1),
                RulUnit = rulUnit,
                Confidence = Math.Round(confidence, 2),
                LowerBound = Math.Round(convertedLower, 1),
                UpperBound = Math.Round(convertedUpper, 1),
                PredictionTime = predictionTime,
                ContributingFactors = contributingFactors,
                ModelVersion = GetModelVersion(),
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                
                // Additional fields for frontend
                DegradationRate = degradationRate,
                Trend = trend,
                EstimatedFailureDate = estimatedFailureDate,
                ModelType = "rul",
                HealthScore = healthScore
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "RUL prediction failed for machine {MachineId}", machineId);
            return CreateFallbackResult(machineId, features, predictionTime);
        }
    }

    public double Predict(IEnumerable<TelemetryData> telemetry)
    {
        var features = ExtractFeaturesFromTelemetry(telemetry);
        return PredictWithFeatures(features);
    }

    /// <inheritdoc />
    public double PredictWithFeatures(Dictionary<string, double> features)
    {
        if (_engine == null)
        {
            return CalculateFallbackRul(features);
        }

        try
        {
            var input = CreateModelInput(features);
            var result = _engine.Predict(input);
            return result.RULDays;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Prediction failed, using fallback");
            return CalculateFallbackRul(features);
        }
    }

    /// <inheritdoc />
    public (double Prediction, double LowerBound, double UpperBound) PredictWithRange(Dictionary<string, double> features)
    {
        var prediction = PredictWithFeatures(features);
        double volatilityScale = CalculateVolatilityScale(features);
        double margin = prediction * volatilityScale;

        if (prediction > 365)
        {
            margin *= 1.2;
        }
        else if (prediction < 30)
        {
            margin *= 0.8;
        }

        return (prediction, Math.Max(0, prediction - margin), prediction + margin);
    }

    /// <inheritdoc />
    public List<ContributingFactor> GetContributingFactors(Dictionary<string, double> features)
    {
        return CalculateContributingFactors(features);
    }

    public void SaveModel(string path)
    {
        if (_model == null)
        {
            _logger.LogWarning("No model loaded to save - model path: {Path}", path);
            return;
        }

        try
        {
            _mlContext.Model.Save(_model, null, path);
            _logger.LogInformation("Model saved to {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save model to {Path}", path);
            throw;
        }
    }

    public void LoadEmbeddedModel(Stream modelStream)
    {
        try
        {
            var loadedModel = _mlContext.Model.Load(modelStream, out var schema);
            _model = loadedModel;
            _engine = _mlContext.Model.CreatePredictionEngine<RulModelInput, RulModelOutput>(loadedModel);
            _logger.LogInformation("Embedded model loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load embedded model");
            throw;
        }
    }

    private PredictionEngine<RulModelInput, RulModelOutput>? LoadPredictionEngine()
    {
        var modelPath = _options.RulModelPath;
        if (!string.IsNullOrWhiteSpace(modelPath) && File.Exists(modelPath))
        {
            try
            {
                var loadedModel = _mlContext.Model.Load(modelPath, out var schema);
                _model = loadedModel;
                return _mlContext.Model.CreatePredictionEngine<RulModelInput, RulModelOutput>(loadedModel);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load model from {Path}, using fallback", modelPath);
            }
        }

        if (_options.UseEmbeddedModels)
        {
            return TryLoadEmbeddedFallback();
        }

        return null;
    }

    private PredictionEngine<RulModelInput, RulModelOutput>? TryLoadEmbeddedFallback()
    {
        var assembly = typeof(RulPredictor).Assembly;
        var resourceNames = assembly.GetManifestResourceNames();
        var rulResource = resourceNames.FirstOrDefault(n => n.Contains("RulModel", StringComparison.OrdinalIgnoreCase));
        if (rulResource != null)
        {
            try
            {
                using var stream = assembly.GetManifestResourceStream(rulResource);
                if (stream != null)
                {
                    var loadedModel = _mlContext.Model.Load(stream, out var schema);
                    _model = loadedModel;
                    _logger.LogInformation("Loaded embedded RUL model from {ResourceName}", rulResource);
                    return _mlContext.Model.CreatePredictionEngine<RulModelInput, RulModelOutput>(loadedModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load embedded model");
            }
        }
        return null;
    }

    private RulModelInput CreateModelInput(Dictionary<string, double> features)
    {
        return new RulModelInput
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

    private double CalculateVolatilityScale(Dictionary<string, double> features)
    {
        double volatilityScale = 0.15;
        if (features.TryGetValue("temp_std", out var tStd) && tStd > 5.0)
            volatilityScale += 0.05;
        if (features.TryGetValue("vib_std", out var vStd) && vStd > 1.0)
            volatilityScale += 0.05;
        if (features.TryGetValue("data_points", out var dp) && dp > 100)
            volatilityScale -= 0.05;
        return Math.Clamp(volatilityScale, 0.05, 0.5);
    }

    private double CalculateConfidence(double lowerBound, double upperBound, double prediction)
    {
        if (prediction <= 0) return 0.5;
        var range = upperBound - lowerBound;
        var relativeRange = range / prediction;
        return Math.Clamp(1.0 - relativeRange, 0.0, 1.0);
    }

    private List<ContributingFactor> CalculateContributingFactors(Dictionary<string, double> features)
    {
        var factors = new List<ContributingFactor>();

        if (features.TryGetValue("temp_mean", out var tempMean) &&
            features.TryGetValue("temp_trend", out var tempTrend))
        {
            var tempImpact = CalculateTemperatureImpact(tempMean, tempTrend);
            factors.Add(new ContributingFactor
            {
                Feature = "temperature",
                Impact = Math.Round(tempImpact, 3),
                Direction = tempTrend > 0.1 ? "negative" : "positive"
            });
        }

        if (features.TryGetValue("vib_mean", out var vibMean) &&
            features.TryGetValue("vib_rms", out var vibRms) &&
            features.TryGetValue("vib_trend", out var vibTrend))
        {
            var vibImpact = CalculateVibrationImpact(vibMean, vibRms, vibTrend);
            factors.Add(new ContributingFactor
            {
                Feature = "vibration",
                Impact = Math.Round(vibImpact, 3),
                Direction = vibTrend > 0.1 ? "negative" : "positive"
            });
        }

        if (features.TryGetValue("pressure_mean", out var pressureMean))
        {
            var pressureImpact = CalculatePressureImpact(pressureMean);
            factors.Add(new ContributingFactor
            {
                Feature = "pressure",
                Impact = Math.Round(pressureImpact, 3),
                Direction = pressureImpact > 0.5 ? "negative" : "positive"
            });
        }

        if (features.TryGetValue("time_span_hours", out var timeSpan) &&
            features.TryGetValue("data_points", out var dataPoints))
        {
            var timeImpact = CalculateTimeImpact(timeSpan, dataPoints);
            factors.Add(new ContributingFactor
            {
                Feature = "time",
                Impact = Math.Round(timeImpact, 3),
                Direction = "negative"
            });
        }

        var totalImpact = factors.Sum(f => f.Impact);
        if (totalImpact > 0)
        {
            foreach (var factor in factors)
            {
                factor.Impact /= totalImpact;
            }
        }

        return factors.OrderByDescending(f => f.Impact).ToList();
    }

    private double CalculateTemperatureImpact(double mean, double trend)
    {
        var baseImpact = 0.0;
        if (mean > _options.HighRiskTemperatureThreshold)
            baseImpact += 0.4;
        else if (mean > 70)
            baseImpact += 0.2;
        if (trend > 0.2)
            baseImpact += 0.3;
        else if (trend > 0.1)
            baseImpact += 0.15;
        return baseImpact;
    }

    private double CalculateVibrationImpact(double mean, double rms, double trend)
    {
        var baseImpact = 0.0;
        if (rms > _options.HighRiskVibrationThreshold)
            baseImpact += 0.4;
        else if (rms > 3.0)
            baseImpact += 0.2;
        if (trend > 0.2)
            baseImpact += 0.3;
        else if (trend > 0.1)
            baseImpact += 0.15;
        return baseImpact;
    }

    private double CalculatePressureImpact(double mean)
    {
        return mean < 90 || mean > 110 ? 0.2 : 0.1;
    }

    private double CalculateTimeImpact(double timeSpan, double dataPoints)
    {
        var density = dataPoints / (timeSpan + 1);
        return Math.Min(0.3, 0.1 + density * 0.01);
    }

    private Dictionary<string, double> InitializeFeatureWeights()
    {
        return new Dictionary<string, double>
        {
            ["temperature"] = _options.FeatureWeights.TryGetValue("temperature", out var t) ? t : 0.25,
            ["vibration"] = _options.FeatureWeights.TryGetValue("vibration", out var v) ? v : 0.35,
            ["pressure"] = _options.FeatureWeights.TryGetValue("pressure", out var p) ? p : 0.15,
            ["time"] = _options.FeatureWeights.TryGetValue("time", out var ti) ? ti : 0.25
        };
    }

    private double ConvertRulToUnit(double days, string unit)
    {
        return unit.ToLowerInvariant() switch
        {
            "hours" => days * 24,
            "days" => days,
            "cycles" => days * 24 * 60,
            _ => days
        };
    }

    private string GetModelVersion() => "1.0.0";

    private RulPredictionResult CreateFallbackResult(string machineId, Dictionary<string, double> features, DateTime predictionTime)
    {
        var prediction = CalculateFallbackRul(features);
        var volatilityScale = CalculateVolatilityScale(features);
        var margin = prediction * volatilityScale;

        // Calculate additional fields for frontend compatibility in fallback
        var degradationRate = CalculateDegradationRate(features);
        var trend = DetermineTrend(degradationRate);
        var estimatedFailureDate = CalculateEstimatedFailureDate(predictionTime, ConvertRulToUnit(prediction, _options.DefaultRulUnit), _options.DefaultRulUnit);
        var healthScore = CalculateHealthScore(prediction, 0.5);

        return new RulPredictionResult
        {
            MachineId = machineId,
            Rul = Math.Round(ConvertRulToUnit(prediction, _options.DefaultRulUnit), 1),
            RulUnit = _options.DefaultRulUnit,
            Confidence = 0.5,
            LowerBound = Math.Round(ConvertRulToUnit(Math.Max(0, prediction - margin), _options.DefaultRulUnit), 1),
            UpperBound = Math.Round(ConvertRulToUnit(prediction + margin, _options.DefaultRulUnit), 1),
            PredictionTime = predictionTime,
            ContributingFactors = CalculateContributingFactors(features),
            ModelVersion = "fallback",
            ProcessingTimeMs = 0,
            
            // Additional fields for frontend
            DegradationRate = degradationRate,
            Trend = trend,
            EstimatedFailureDate = estimatedFailureDate,
            ModelType = "rul",
            HealthScore = healthScore
        };
    }

    private static double CalculateFallbackRul(Dictionary<string, double> features)
    {
        var baseRul = 365.0;
        if (features.TryGetValue("temp_mean", out var tempMean) && tempMean > 80)
        {
            baseRul *= Math.Max(0.5, 1.0 - (tempMean - 80) / 100);
        }
        if (features.TryGetValue("vib_rms", out var vibRms) && vibRms > 5.0)
        {
            baseRul *= Math.Max(0.3, 1.0 - (vibRms - 5.0) / 20);
        }
        if (features.TryGetValue("temp_trend", out var tempTrend) && tempTrend > 0.1)
        {
            baseRul *= 0.8;
        }
        if (features.TryGetValue("vib_trend", out var vibTrend) && vibTrend > 0.1)
        {
            baseRul *= 0.7;
        }
        return Math.Max(1.0, baseRul);
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

    private double CalculateDegradationRate(Dictionary<string, double> features)
    {
        // Calculate degradation rate based on feature trends and values
        var degradationRate = 0.0;
        
        if (features.TryGetValue("temp_trend", out var tempTrend))
        {
            degradationRate += Math.Abs(tempTrend) * 0.3; // Temperature trend contributes significantly
        }
        
        if (features.TryGetValue("vib_trend", out var vibTrend))
        {
            degradationRate += Math.Abs(vibTrend) * 0.4; // Vibration trend contributes significantly
        }
        
        if (features.TryGetValue("temp_mean", out var tempMean) && tempMean > 70)
        {
            degradationRate += (tempMean - 70) * 0.01; // Higher temperature increases degradation
        }
        
        if (features.TryGetValue("vib_rms", out var vibRms) && vibRms > 2.0)
        {
            degradationRate += (vibRms - 2.0) * 0.02; // Higher vibration increases degradation
        }
        
        return Math.Round(degradationRate, 3);
    }

    private string DetermineTrend(double degradationRate)
    {
        // Determine the trend based on degradation rate
        if (degradationRate > 0.5)
        {
            return "declining"; // Rapid deterioration
        }
        else if (degradationRate > 0.1)
        {
            return "declining"; // Moderate deterioration
        }
        else if (degradationRate < -0.1)
        {
            return "improving"; // Improving condition
        }
        else
        {
            return "stable"; // Stable condition
        }
    }

    private DateTime CalculateEstimatedFailureDate(DateTime predictionTime, double rul, string rulUnit)
    {
        // Calculate estimated failure date based on RUL and unit
        var timeToAdd = TimeSpan.Zero;
        
        switch (rulUnit.ToLower())
        {
            case "hours":
                timeToAdd = TimeSpan.FromHours(rul);
                break;
            case "days":
                timeToAdd = TimeSpan.FromDays(rul);
                break;
            case "cycles":
                // Assume 24 cycles per day for estimation
                timeToAdd = TimeSpan.FromDays(rul / 24.0);
                break;
            default:
                timeToAdd = TimeSpan.FromDays(rul); // Default to days
                break;
        }
        
        return predictionTime.Add(timeToAdd);
    }

    private double CalculateHealthScore(double rul, double confidence)
    {
        // Calculate health score based on RUL and confidence
        // Higher RUL and confidence = higher health score
        var baseScore = Math.Min(1.0, rul / 365.0); // Normalize RUL against 1 year
        var adjustedScore = baseScore * confidence; // Factor in confidence
        
        // Ensure the score is within bounds
        return Math.Clamp(adjustedScore, 0.0, 1.0);
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

/// <summary>
/// Input data class for ML.NET RUL model.
/// </summary>
public class RulModelInput
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

/// <summary>
/// Output data class for ML.NET RUL model.
/// </summary>
public class RulModelOutput
{
    public float RULDays { get; set; }
}
