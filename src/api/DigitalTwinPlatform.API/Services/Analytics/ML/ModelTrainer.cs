using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Enums;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Abstractions.Analytics;
using DigitalTwinPlatform.API.Services.Analytics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Text.Json;
using DigitalTwinPlatform.Domain.Entities.Enums;

namespace DigitalTwinPlatform.API.Services.Analytics.ML;

public class ModelTrainer
{
    private readonly MLContext _ml;
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly IPredictionRepository _predictionRepository;
    private readonly IMlExperimentLogger _experimentLogger;
    private readonly IModelLifecycleService? _lifecycleService;
    private readonly IOptions<MlOptions> _options;
    private readonly ILogger<ModelTrainer> _logger;

    public ModelTrainer(
        ITelemetryRepository telemetryRepository,
        IPredictionRepository predictionRepository,
        IMlExperimentLogger experimentLogger,
        IOptions<MlOptions> options,
        ILogger<ModelTrainer> logger,
        IModelLifecycleService? lifecycleService = null)
    {
        _ml = new MLContext(seed: 0);
        _telemetryRepository = telemetryRepository;
        _predictionRepository = predictionRepository;
        _experimentLogger = experimentLogger;
        _lifecycleService = lifecycleService;
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    
        // Ensure model directory exists
        var modelDir = Path.GetDirectoryName(_options.Value.RulModelPath ?? "./models/model.zip");
        if (!string.IsNullOrEmpty(modelDir))
        {
            Directory.CreateDirectory(modelDir);
        }
    }

    public async Task TrainAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Starting ML.NET model training pipeline");

            // Prepare training data
            var trainingData = await PrepareTrainingDataAsync(ct);
            
            if (trainingData.Count == 0)
            {
                _logger.LogWarning("No training data available. Skipping training.");
                return;
            }

            // Train RUL prediction model
            await TrainRulModelAsync(trainingData);

            // Train health classification model
            await TrainHealthModelAsync(trainingData);

            _logger.LogInformation("ML.NET model training completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ML.NET model training failed");
            throw;
        }
    }

    private async Task<List<TrainingSample>> PrepareTrainingDataAsync(CancellationToken ct)
    {
        var trainingSamples = new List<TrainingSample>();
        
        // Get all machines with sufficient telemetry history
        var machines = await _telemetryRepository.GetAllAsync(m => true, ct);
        var machineGroups = machines.GroupBy(m => m.MachineId);

        foreach (var machineGroup in machineGroups)
        {
            var machineTelemetry = machineGroup.OrderByDescending(t => t.Timestamp).Take(100).ToList();
            
            if (machineTelemetry.Count < 20) // Need minimum data points
                continue;

            // Extract features from telemetry
            var features = ExtractFeaturesFromTelemetry(machineTelemetry);
            
            // Get corresponding predictions for supervised learning
            var predictions = await _predictionRepository.GetAllAsync(p => p.MachineId == machineGroup.Key, ct);
            var latestPrediction = predictions.OrderByDescending(p => p.CreatedAt).FirstOrDefault();

            if (latestPrediction != null)
            {
                trainingSamples.Add(new TrainingSample
                {
                    Features = features,
                    RulDays = (float)latestPrediction.RemainingUsefulLifeDays,
                    HealthStatus = latestPrediction.HealthStatus,
                    FailureProbability = (float)latestPrediction.FailureProbability
                });
            }
        }

        return trainingSamples;
    }

    /// <summary>
    /// Extracts statistical features from telemetry data for model training.
    /// </summary>
    private static Dictionary<string, double> ExtractFeaturesFromTelemetry(IEnumerable<TelemetryData> telemetry)
    {
        var telemetryList = telemetry.ToList();
        var features = new Dictionary<string, double>();

        ExtractTemperatureFeatures(telemetryList, features);
        ExtractVibrationFeatures(telemetryList, features);
        ExtractPressureFeatures(telemetryList, features);
        ExtractTimeBasedFeatures(telemetryList, features);

        return features;
    }

    /// <summary>
    /// Extracts temperature-related statistical features.
    /// </summary>
    private static void ExtractTemperatureFeatures(
        List<TelemetryData> telemetryList,
        Dictionary<string, double> features)
    {
        var tempData = telemetryList.Where(t => t.DataType == "temperature").ToList();
        if (tempData.Count == 0) return;

        var temps = ExtractNumericValues(tempData, "value").ToList();
        if (temps.Count == 0) return;

        var mean = temps.Average();
        features["temp_mean"] = mean;
        features["temp_std"] = CalculateStandardDeviation(temps, mean);
        features["temp_max"] = temps.Max();
        features["temp_min"] = temps.Min();
        features["temp_trend"] = CalculateTrend(temps);
    }

    /// <summary>
    /// Extracts vibration-related statistical features.
    /// </summary>
    private static void ExtractVibrationFeatures(
        List<TelemetryData> telemetryList,
        Dictionary<string, double> features)
    {
        var vibData = telemetryList.Where(t => t.DataType == "vibration").ToList();
        if (vibData.Count == 0) return;

        var vibes = ExtractNumericValues(vibData, "amplitude").ToList();
        if (vibes.Count == 0) return;

        var mean = vibes.Average();
        features["vib_mean"] = mean;
        features["vib_std"] = CalculateStandardDeviation(vibes, mean);
        features["vib_max"] = vibes.Max();
        features["vib_rms"] = Math.Sqrt(vibes.Average(v => v * v));
        features["vib_trend"] = CalculateTrend(vibes);
    }

    /// <summary>
    /// Extracts pressure-related statistical features.
    /// </summary>
    private static void ExtractPressureFeatures(
        List<TelemetryData> telemetryList,
        Dictionary<string, double> features)
    {
        var pressureData = telemetryList.Where(t => t.DataType == "pressure").ToList();
        if (pressureData.Count == 0) return;

        var pressures = ExtractNumericValues(pressureData, "value").ToList();
        if (pressures.Count == 0) return;

        var mean = pressures.Average();
        features["pressure_mean"] = mean;
        features["pressure_std"] = CalculateStandardDeviation(pressures, mean);
        features["pressure_trend"] = CalculateTrend(pressures);
    }

    /// <summary>
    /// Extracts time-based features from telemetry data.
    /// </summary>
    private static void ExtractTimeBasedFeatures(
        List<TelemetryData> telemetryList,
        Dictionary<string, double> features)
    {
        features["data_points"] = telemetryList.Count;
        features["time_span_hours"] = telemetryList.Count > 0
            ? (telemetryList.Max(t => t.Timestamp) - telemetryList.Min(t => t.Timestamp)).TotalHours
            : 0;
    }

    /// <summary>
    /// Extracts numeric values from telemetry data for a specific property.
    /// </summary>
    private static IEnumerable<double> ExtractNumericValues(
        IEnumerable<TelemetryData> telemetry,
        string propertyName)
    {
        return telemetry
            .SelectMany(t => TryGetDouble(t.Data, propertyName, out var value) ? [value] : Array.Empty<double>());
    }

    /// <summary>
    /// Calculates the standard deviation of a collection of values.
    /// </summary>
    private static double CalculateStandardDeviation(IEnumerable<double> values, double mean)
    {
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

    private static bool TryGetDouble(JsonDocument doc, string propertyName, out double value)
    {
        value = 0;
        if (doc.RootElement.ValueKind != JsonValueKind.Object) return false;
        if (!doc.RootElement.TryGetProperty(propertyName, out var element)) return false;
        return element.TryGetDouble(out value);
    }

    /// <summary>
    /// Trains the RUL (Remaining Useful Life) prediction model.
    /// </summary>
    private Task TrainRulModelAsync(List<TrainingSample> trainingData)
    {
        var mlData = ConvertToRulTrainingData(trainingData);
        var dataView = _ml.Data.LoadFromEnumerable(mlData);
        var pipeline = CreateRulTrainingPipeline();
        var model = pipeline.Fit(dataView);
        var (metrics, mape) = EvaluateRulModel(model, dataView);
        var modelPath = SaveRulModel(model, dataView.Schema);
        
        LogAndRegisterRulModel(metrics, mape, modelPath, trainingData.Count);
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Converts training samples to ML.NET RUL training data format.
    /// </summary>
    private static List<RulTrainingData> ConvertToRulTrainingData(List<TrainingSample> trainingData)
    {
        return trainingData.Select(s => new RulTrainingData
        {
            Feature1 = s.Features.TryGetValue("data_points", out var dp) ? (float)dp : 0f,
            Feature2 = s.Features.TryGetValue("temp_mean", out var tm) ? (float)tm : 0f,
            Feature3 = s.Features.TryGetValue("vib_mean", out var vm) ? (float)vm : 0f,
            Feature4 = s.Features.TryGetValue("vib_rms", out var vr) ? (float)vr : 0f,
            Feature5 = s.Features.TryGetValue("temp_std", out var ts) ? (float)ts : 0f,
            Feature6 = s.Features.TryGetValue("vib_std", out var vs) ? (float)vs : 0f,
            Feature7 = s.Features.TryGetValue("temp_trend", out var tt) ? (float)tt : 0f,
            Feature8 = s.Features.TryGetValue("vib_trend", out var vt) ? (float)vt : 0f,
            Feature9 = s.Features.TryGetValue("pressure_mean", out var pm) ? (float)pm : 0f,
            Feature10 = s.Features.TryGetValue("time_span_hours", out var th) ? (float)th : 0f,
            RulDays = s.RulDays
        }).ToList();
    }

    /// <summary>
    /// Creates the ML.NET training pipeline for RUL prediction.
    /// </summary>
    private IEstimator<ITransformer> CreateRulTrainingPipeline()
    {
        return _ml.Transforms.Concatenate("Features", 
                "Feature1", "Feature2", "Feature3", "Feature4", "Feature5", 
                "Feature6", "Feature7", "Feature8", "Feature9", "Feature10")
            .Append(_ml.Regression.Trainers.LightGbm(labelColumnName: "RulDays", featureColumnName: "Features"));
    }

    /// <summary>
    /// Evaluates the RUL model and returns metrics.
    /// </summary>
    private (RegressionMetrics Metrics, double Mape) EvaluateRulModel(ITransformer model, IDataView dataView)
    {
        var predictions = model.Transform(dataView);
        var metrics = _ml.Regression.Evaluate(predictions, labelColumnName: "RulDays");
        var mape = CalculateMape(predictions, "RulDays", "Score");
        return (metrics, mape);
    }

    /// <summary>
    /// Saves the trained RUL model with versioning.
    /// </summary>
    private string SaveRulModel(ITransformer model, DataViewSchema schema)
    {
        var modelPath = GetVersionedModelPath("rul");
        _ml.Model.Save(model, schema, modelPath);
        return modelPath;
    }

    /// <summary>
    /// Logs training metrics and registers the model in lifecycle management.
    /// </summary>
    private void LogAndRegisterRulModel(
        RegressionMetrics metrics,
        double mape,
        string modelPath,
        int trainingSampleCount)
    {
        _logger.LogInformation("RUL model trained. R2: {R2:F4}, MAPE: {MAPE:P2}", metrics.RSquared, mape);

        _ = _experimentLogger.LogExperimentAsync("RUL", Path.GetFileName(modelPath), metrics.RSquared, mape);

        if (_lifecycleService != null)
        {
            RegisterRulModelInLifecycle(modelPath, metrics, mape, trainingSampleCount);
        }
    }

    /// <summary>
    /// Registers a RUL model version in the lifecycle management service.
    /// </summary>
    private void RegisterRulModelInLifecycle(
        string modelPath,
        RegressionMetrics metrics,
        double mape,
        int trainingSampleCount)
    {
        try
        {
            var modelMetrics = new Dictionary<string, double>
            {
                ["R2"] = metrics.RSquared,
                ["MAPE"] = mape,
                ["MAE"] = metrics.MeanAbsoluteError,
                ["RMSE"] = metrics.RootMeanSquaredError
            };

            _ = _lifecycleService!.RegisterModelVersionAsync(
                "RUL",
                modelPath,
                modelMetrics,
                notes: $"Trained on {trainingSampleCount} samples");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to register model version in lifecycle service");
        }
    }

    private double CalculateMape(IDataView predictions, string labelColumn, string scoreColumn)
    {
        var labels = predictions.GetColumn<float>(labelColumn).ToArray();
        var scores = predictions.GetColumn<float>(scoreColumn).ToArray();

        double sumPercentageError = 0;
        int count = 0;

        for (int i = 0; i < labels.Length; i++)
        {
            if (Math.Abs(labels[i]) > 0.001f) // Avoid division by zero
            {
                sumPercentageError += Math.Abs((labels[i] - scores[i]) / labels[i]);
                count++;
            }
        }

        return count > 0 ? sumPercentageError / count : 0;
    }

    private Task TrainHealthModelAsync(List<TrainingSample> trainingData)
    {
        // Convert to ML.NET data format
        var mlData = trainingData.Select(s => new HealthTrainingData
        {
            Feature1 = s.Features.TryGetValue("data_points", out var dp) ? (float)dp : 0f,
            Feature2 = s.Features.TryGetValue("temp_mean", out var tm) ? (float)tm : 0f,
            Feature3 = s.Features.TryGetValue("vib_mean", out var vm) ? (float)vm : 0f,
            Feature4 = s.Features.TryGetValue("vib_rms", out var vr) ? (float)vr : 0f,
            Feature5 = s.Features.TryGetValue("temp_std", out var ts) ? (float)ts : 0f,
            Feature6 = s.Features.TryGetValue("vib_std", out var vs) ? (float)vs : 0f,
            Feature7 = s.Features.TryGetValue("temp_trend", out var tt) ? (float)tt : 0f,
            Feature8 = s.Features.TryGetValue("vib_trend", out var vt) ? (float)vt : 0f,
            Feature9 = s.Features.TryGetValue("pressure_mean", out var pm) ? (float)pm : 0f,
            Feature10 = s.Features.TryGetValue("time_span_hours", out var th) ? (float)th : 0f,
            HealthStatus = s.HealthStatus.ToString(),
            FailureProbability = s.FailureProbability
        }).ToList();

        var dataView = _ml.Data.LoadFromEnumerable(mlData);

        // Define training pipeline for classification using SdcaMaximumEntropy (highly performant)
        var pipeline = _ml.Transforms.Concatenate("Features",
                "Feature1", "Feature2", "Feature3", "Feature4", "Feature5",
                "Feature6", "Feature7", "Feature8", "Feature9", "Feature10")
            .Append(_ml.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", inputColumnName: "HealthStatus"))
            .Append(_ml.MulticlassClassification.Trainers.SdcaMaximumEntropy())
            .Append(_ml.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        // Train the model
        var model = pipeline.Fit(dataView);

        // Evaluate the model
        var predictions = model.Transform(dataView);
        var metrics = _ml.MulticlassClassification.Evaluate(predictions, labelColumnName: "Label");

        // Save the model with versioning
        var modelPath = GetVersionedModelPath("health");
        _ml.Model.Save(model, dataView.Schema, modelPath);

        _logger.LogInformation("Health model trained and saved to {ModelPath}. Accuracy: {Accuracy:F4}", modelPath, metrics.MacroAccuracy);

        // Register model version in lifecycle management
        if (_lifecycleService != null)
        {
            try
            {
                var modelMetrics = new Dictionary<string, double>
                {
                    ["Accuracy"] = metrics.MacroAccuracy,
                    ["MicroAccuracy"] = metrics.MicroAccuracy,
                    ["LogLoss"] = metrics.LogLoss
                };

                _ = _lifecycleService.RegisterModelVersionAsync(
                    "Health",
                    modelPath,
                    modelMetrics,
                    notes: $"Trained on {trainingData.Count} samples");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to register model version in lifecycle service");
            }
        }

        return Task.CompletedTask;
    }

    private string GetVersionedModelPath(string modelType)
    {
        var version = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var directory = Path.GetDirectoryName(_options.Value.RulModelPath) ?? "./models";
        Directory.CreateDirectory(directory);
        
        return modelType.ToLower() switch
        {
            "rul" => Path.Combine(directory, $"rul-model-{version}.zip"),
            "health" => Path.Combine(directory, $"health-model-{version}.zip"),
            _ => Path.Combine(directory, $"{modelType}-model-{version}.zip")
        };
    }

    public Task<bool> ValidateModelAsync(string modelPath)
    {
        try
        {
            if (!File.Exists(modelPath))
                return Task.FromResult(false);

            // Load model and run basic validation
            var model = _ml.Model.Load(modelPath, out var schema);
            
            // Create test data for validation
            var testData = new List<RulTrainingData>
            {
                new() { Feature1 = 50, Feature2 = 75, Feature3 = 3, Feature4 = 4, Feature5 = 2, Feature6 = 1, Feature7 = 0.1f, Feature8 = 0.05f, Feature9 = 100, Feature10 = 24, RulDays = 180 }
            };

            var testDataView = _ml.Data.LoadFromEnumerable(testData);
            var predictions = model.Transform(testDataView);
            var metrics = _ml.Regression.Evaluate(predictions, labelColumnName: "RulDays");

            _logger.LogInformation("Model validation - R2: {R2:F3}, MAE: {MAE:F3}", metrics.RSquared, metrics.MeanAbsoluteError);

            return Task.FromResult(metrics.RSquared > 0.5); // Basic quality threshold
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Model validation failed for {ModelPath}", modelPath);
            return Task.FromResult(false);
        }
    }
}

// Training data classes
public class TrainingSample
{
    public Dictionary<string, double> Features { get; init; } = [];
    public float RulDays { get; init; }
    public HealthClassification HealthStatus { get; init; }
    public float FailureProbability { get; init; }
}

public class RulTrainingData
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
    public float RulDays { get; set; }
}

public class HealthTrainingData
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
    public string HealthStatus { get; set; } = string.Empty;
    public float FailureProbability { get; set; }
}
