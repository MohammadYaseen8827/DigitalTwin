using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Entities.Enums;
using DigitalTwinPlatform.Domain.Enums;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Abstractions.Analytics;
using DigitalTwinPlatform.API.Services.Simulation;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Text.Json;

namespace DigitalTwinPlatform.API.Services.Analytics;

public interface IModelRetrainingService
{
    Task<bool> StartRetrainingAsync(Guid machineId, CancellationToken ct = default);
    Task<bool> CheckDriftAndRetrainAsync(Guid machineId, IEnumerable<double> recentData, string benchmarkDataset, CancellationToken ct = default);
}

public class ModelRetrainingService : IModelRetrainingService
{
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly IPredictionRepository _predictionRepository;
    private readonly IRepository<ModelVersion> _modelVersionRepository;
    private readonly IRepository<Machine> _machineRepository;
    private readonly IDataValidationService _validationService;
    private readonly IMlExperimentLogger _experimentLogger;
    private readonly ILogger<ModelRetrainingService> _logger;
    private readonly MLContext _mlContext;
    private readonly string _modelStoragePath;

    public ModelRetrainingService(
        ITelemetryRepository telemetryRepository,
        IPredictionRepository predictionRepository,
        IRepository<ModelVersion> modelVersionRepository,
        IRepository<Machine> machineRepository,
        IDataValidationService validationService,
        IMlExperimentLogger experimentLogger,
        ILogger<ModelRetrainingService> logger,
        IConfiguration configuration)
    {
        _telemetryRepository = telemetryRepository ?? throw new ArgumentNullException(nameof(telemetryRepository));
        _predictionRepository = predictionRepository ?? throw new ArgumentNullException(nameof(predictionRepository));
        _modelVersionRepository = modelVersionRepository ?? throw new ArgumentNullException(nameof(modelVersionRepository));
        _machineRepository = machineRepository ?? throw new ArgumentNullException(nameof(machineRepository));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _experimentLogger = experimentLogger ?? throw new ArgumentNullException(nameof(experimentLogger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mlContext = new MLContext(seed: 42);
        _modelStoragePath = configuration["ML:ModelStoragePath"] ?? "./models";
        
        Directory.CreateDirectory(_modelStoragePath);
    }

    public async Task<bool> CheckDriftAndRetrainAsync(Guid machineId, IEnumerable<double> recentData, string benchmarkDataset, CancellationToken ct = default)
    {
        _logger.LogInformation("Checking for distribution drift for machine {MachineId}...", machineId);
        
        try
        {
            var validationResult = _validationService.ValidateAgainstBenchmark(recentData, benchmarkDataset);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Significant drift detected for {MachineId} (MMD: {Mmd:F4}, KL: {Kl:F4}). Triggering auto-retraining...", 
                    machineId, validationResult.MmdStatistic, validationResult.KlDivergence);
                
                return await StartRetrainingAsync(machineId, ct);
            }

            _logger.LogInformation("No significant drift detected for machine {MachineId}.", machineId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Drift check failed for machine {MachineId}", machineId);
            return false;
        }
    }

    public async Task<bool> StartRetrainingAsync(Guid machineId, CancellationToken ct = default)
    {
        _logger.LogInformation("Initiating model retraining for machine {MachineId}...", machineId);
        
        try
        {
            var machine = await _machineRepository.GetAsync(machineId, ct);
            if (machine == null)
            {
                _logger.LogError("Machine {MachineId} not found", machineId);
                return false;
            }

            var trainingData = await PrepareTrainingDataAsync(machineId, ct);
            if (trainingData.Count < 20)
            {
                _logger.LogWarning("Insufficient training data for machine {MachineId}. Minimum 20 samples required, found {Count}", 
                    machineId, trainingData.Count);
                return false;
            }

            _logger.LogInformation("Fetched {Count} training samples for machine {MachineId}", trainingData.Count, machineId);

            var (rulModelPath, rulMetrics) = await TrainRulModelAsync(trainingData, machineId, ct);
            if (string.IsNullOrEmpty(rulModelPath))
            {
                _logger.LogError("RUL model training failed for machine {MachineId}", machineId);
                return false;
            }

            _logger.LogInformation("RUL model trained for {MachineId}. R2: {R2:F4}, MAE: {MAE:F2}", 
                machineId, rulMetrics?.RSquared ?? 0, rulMetrics?.MeanAbsoluteError ?? 0);

            var (healthModelPath, healthMetrics) = await TrainHealthModelAsync(trainingData, machineId, ct);
            if (string.IsNullOrEmpty(healthModelPath))
            {
                _logger.LogError("Health model training failed for machine {MachineId}", machineId);
                return false;
            }

            _logger.LogInformation("Health model trained for {MachineId}. Accuracy: {Accuracy:F4}", 
                machineId, healthMetrics?.MacroAccuracy ?? 0);

            var version = await RegisterModelVersionAsync(machineId, rulModelPath, rulMetrics, trainingData.Count, ct);

            await _experimentLogger.LogExperimentAsync(
                "RUL_Retraining",
                version.Version,
                rulMetrics?.RSquared ?? 0,
                CalculateMape(rulMetrics));

            _logger.LogInformation("Model retraining completed successfully for machine {MachineId}. Version: {Version}", 
                machineId, version.Version);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Model retraining failed for machine {MachineId}", machineId);
            return false;
        }
    }

    private async Task<List<TrainingSample>> PrepareTrainingDataAsync(Guid machineId, CancellationToken ct)
    {
        var samples = new List<TrainingSample>();
        
        var telemetry = await _telemetryRepository.GetForMachineAsync(machineId, since: DateTime.UtcNow.AddMonths(-6), take: 1000, ct: ct);
        var telemetryList = telemetry.ToList();
        
        if (telemetryList.Count < 20)
        {
            _logger.LogWarning("Insufficient telemetry data for machine {MachineId}", machineId);
            return samples;
        }

        var predictions = await _predictionRepository.GetByMachineIdAsync(machineId, take: 100, ct: ct);
        var predictionList = predictions.ToList();

        var timeWindows = telemetryList
            .GroupBy(t => new { t.Timestamp.Date, Hour = t.Timestamp.Hour / 4 })
            .Where(g => g.Count() >= 5);

        foreach (var window in timeWindows)
        {
            var windowTelemetry = window.ToList();
            var windowTime = window.Key.Date.AddHours(window.Key.Hour * 4);
            
            var matchingPrediction = predictionList
                .OrderBy(p => Math.Abs((p.CreatedAt - windowTime).TotalHours))
                .FirstOrDefault();

            if (matchingPrediction == null) continue;

            var features = ExtractFeatures(windowTelemetry);
            if (features.Count > 0)
            {
                samples.Add(new TrainingSample
                {
                    Features = features,
                    RulDays = (float)matchingPrediction.RemainingUsefulLifeDays,
                    HealthStatus = matchingPrediction.HealthStatus,
                    FailureProbability = (float)matchingPrediction.FailureProbability,
                    Timestamp = windowTime
                });
            }
        }

        return samples;
    }

    private Dictionary<string, double> ExtractFeatures(List<TelemetryData> telemetry)
    {
        var features = new Dictionary<string, double>();

        var tempData = telemetry.Where(t => t.DataType == "temperature").ToList();
        if (tempData.Count > 0)
        {
            var temps = tempData.Select(t => GetNumericValue(t.Data, "value")).Where(v => v.HasValue).Select(v => v!.Value).ToList();
            if (temps.Count > 0)
            {
                features["temp_mean"] = temps.Average();
                features["temp_std"] = CalculateStdDev(temps);
                features["temp_max"] = temps.Max();
                features["temp_min"] = temps.Min();
                features["temp_trend"] = CalculateTrend(temps);
            }
        }

        var vibData = telemetry.Where(t => t.DataType == "vibration").ToList();
        if (vibData.Count > 0)
        {
            var vibes = vibData.Select(t => GetNumericValue(t.Data, "amplitude")).Where(v => v.HasValue).Select(v => v!.Value).ToList();
            if (vibes.Count > 0)
            {
                features["vib_mean"] = vibes.Average();
                features["vib_std"] = CalculateStdDev(vibes);
                features["vib_max"] = vibes.Max();
                features["vib_rms"] = Math.Sqrt(vibes.Average(v => v * v));
                features["vib_trend"] = CalculateTrend(vibes);
            }
        }

        var pressureData = telemetry.Where(t => t.DataType == "pressure").ToList();
        if (pressureData.Count > 0)
        {
            var pressures = pressureData.Select(t => GetNumericValue(t.Data, "value")).Where(v => v.HasValue).Select(v => v!.Value).ToList();
            if (pressures.Count > 0)
            {
                features["pressure_mean"] = pressures.Average();
                features["pressure_std"] = CalculateStdDev(pressures);
                features["pressure_trend"] = CalculateTrend(pressures);
            }
        }

        var rpmData = telemetry.Where(t => t.DataType == "rpm" || t.DataType == "rotation").ToList();
        if (rpmData.Count > 0)
        {
            var rpms = rpmData.Select(t => GetNumericValue(t.Data, "value")).Where(v => v.HasValue).Select(v => v!.Value).ToList();
            if (rpms.Count > 0)
            {
                features["rpm_mean"] = rpms.Average();
                features["rpm_std"] = CalculateStdDev(rpms);
            }
        }

        features["data_points"] = telemetry.Count;
        features["time_span_hours"] = (telemetry.Max(t => t.Timestamp) - telemetry.Min(t => t.Timestamp)).TotalHours;

        return features;
    }

    private double? GetNumericValue(JsonDocument data, string propertyName)
    {
        if (data.RootElement.ValueKind != JsonValueKind.Object) return null;
        if (!data.RootElement.TryGetProperty(propertyName, out var element)) return null;
        return element.TryGetDouble(out var value) ? value : null;
    }

    private double CalculateStdDev(List<double> values)
    {
        if (values.Count < 2) return 0;
        var mean = values.Average();
        return Math.Sqrt(values.Average(v => Math.Pow(v - mean, 2)));
    }

    private double CalculateTrend(List<double> values)
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

    private async Task<(string? Path, RegressionMetrics? Metrics)> TrainRulModelAsync(
        List<TrainingSample> trainingData, Guid machineId, CancellationToken ct)
    {
        try
        {
            await Task.Yield();
            var mlData = ConvertToRulTrainingData(trainingData);
            var dataView = _mlContext.Data.LoadFromEnumerable(mlData);

            var pipeline = _mlContext.Transforms.Concatenate("Features",
                    "Feature1", "Feature2", "Feature3", "Feature4", "Feature5",
                    "Feature6", "Feature7", "Feature8", "Feature9", "Feature10")
                .Append(_mlContext.Regression.Trainers.LightGbm(
                    labelColumnName: "RulDays",
                    featureColumnName: "Features",
                    numberOfLeaves: 20,
                    minimumExampleCountPerLeaf: 10,
                    learningRate: 0.05));

            var model = pipeline.Fit(dataView);
            var predictions = model.Transform(dataView);
            var metrics = _mlContext.Regression.Evaluate(predictions, labelColumnName: "RulDays");

            var version = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var modelPath = Path.Combine(_modelStoragePath, $"rul-{machineId:N}-{version}.zip");
            _mlContext.Model.Save(model, dataView.Schema, modelPath);

            return (modelPath, metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RUL model training failed");
            return (null, null);
        }
    }

    private async Task<(string? Path, MulticlassClassificationMetrics? Metrics)> TrainHealthModelAsync(
        List<TrainingSample> trainingData, Guid machineId, CancellationToken ct)
    {
        try
        {
            await Task.Yield();
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
                HealthStatus = s.HealthStatus.ToString()
            }).ToList();

            var dataView = _mlContext.Data.LoadFromEnumerable(mlData);

            var pipeline = _mlContext.Transforms.Concatenate("Features",
                    "Feature1", "Feature2", "Feature3", "Feature4", "Feature5",
                    "Feature6", "Feature7", "Feature8", "Feature9", "Feature10")
                .Append(_mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", inputColumnName: "HealthStatus"))
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var model = pipeline.Fit(dataView);
            var predictions = model.Transform(dataView);
            var metrics = _mlContext.MulticlassClassification.Evaluate(predictions, labelColumnName: "Label");

            var version = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var modelPath = Path.Combine(_modelStoragePath, $"health-{machineId:N}-{version}.zip");
            _mlContext.Model.Save(model, dataView.Schema, modelPath);

            return (modelPath, metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health model training failed");
            return (null, null);
        }
    }

    private async Task<ModelVersion> RegisterModelVersionAsync(
        Guid machineId, string modelPath, RegressionMetrics? metrics, int sampleCount, CancellationToken ct)
    {
        var version = new ModelVersion
        {
            Id = Guid.NewGuid(),
            ModelType = "RUL",
            Version = $"v2.{DateTime.UtcNow:yyyyMMdd.HHmm}",
            ModelPath = modelPath,
            TrainedAt = DateTime.UtcNow,
            Status = ModelStatus.Production,
            CreatedAt = DateTime.UtcNow,
            Metrics = new Dictionary<string, double>
            {
                ["R2"] = metrics?.RSquared ?? 0,
                ["MAE"] = metrics?.MeanAbsoluteError ?? 0,
                ["RMSE"] = metrics?.RootMeanSquaredError ?? 0,
                ["TrainingSamples"] = sampleCount
            }
        };

        await _modelVersionRepository.AddAsync(version, ct);
        await _modelVersionRepository.SaveChangesAsync(ct);

        return version;
    }

    private List<RulTrainingData> ConvertToRulTrainingData(List<TrainingSample> trainingData)
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

    private double CalculateMape(RegressionMetrics? metrics)
    {
        if (metrics == null) return 0;
        return metrics.MeanAbsoluteError / 365.0;
    }
}

public class TrainingSample
{
    public Dictionary<string, double> Features { get; init; } = new();
    public float RulDays { get; init; }
    public HealthClassification HealthStatus { get; init; }
    public float FailureProbability { get; init; }
    public DateTime Timestamp { get; init; }
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
}
