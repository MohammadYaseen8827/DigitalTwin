using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Abstractions.Services;
using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Predictions.Models;
using DigitalTwinPlatform.API.Services.Analytics.ML;
using DigitalTwinPlatform.API.Services.Core;
using DigitalTwinPlatform.API.Services.Infrastructure;
using DigitalTwinPlatform.Domain.Constants;
using Microsoft.Extensions.Options;
using System.Text.Json;
using DigitalTwinPlatform.Domain.Entities.Enums;
using DigitalTwinPlatform.Application.Services;

namespace DigitalTwinPlatform.API.Services.Analytics;

public class PredictiveAnalyticsService : IPredictiveAnalyticsService
{
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly IRepository<Prediction> _predictionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DigitalTwinPlatform.Application.Abstractions.Services.ITwinEngineService _twinEngineService;
    private readonly RulPredictor _rulPredictor;
    private readonly HealthClassifier _healthClassifier;
    private readonly IPredictiveXaiService _xaiService;
    private readonly IAlertService _alertService;
    private readonly IOptions<MlOptions> _mlOptions;

    public PredictiveAnalyticsService(
        ITelemetryRepository telemetryRepository,
        IRepository<Prediction> predictionRepository,
        IUnitOfWork unitOfWork,
        DigitalTwinPlatform.Application.Abstractions.Services.ITwinEngineService twinEngineService,
        RulPredictor rulPredictor,
        HealthClassifier healthClassifier,
        IPredictiveXaiService xaiService,
        IAlertService alertService,
        IOptions<MlOptions> mlOptions)
    {
        _telemetryRepository = telemetryRepository;
        _predictionRepository = predictionRepository;
        _unitOfWork = unitOfWork;
        _twinEngineService = twinEngineService;
        _rulPredictor = rulPredictor;
        _healthClassifier = healthClassifier;
        _xaiService = xaiService;
        _alertService = alertService;
        _mlOptions = mlOptions;
    }

    public async Task<PredictionDto> PredictAsync(Guid machineId)
    {
        // Fetch recent telemetry and run actual ML.NET inference
        var telemetry = (await _telemetryRepository.GetForMachineAsync(machineId, null, 50)).ToList();
        
        if (!telemetry.Any())
        {
            // Return default prediction if no telemetry available
            var defaultPrediction = new Prediction
            {
                Id = Guid.NewGuid(),
                MachineId = machineId,
                RemainingUsefulLifeDays = 365.0,
                FailureProbability = 0.1,
                HealthStatus = HealthClassification.Healthy,
                CreatedAt = DateTime.UtcNow,
                ModelVersion = "v0-default"
            };

            await _predictionRepository.AddAsync(defaultPrediction);
            await _unitOfWork.SaveChangesAsync();
            return new PredictionDto(
                defaultPrediction.Id, 
                defaultPrediction.MachineId, 
                defaultPrediction.RemainingUsefulLifeDays, 
                defaultPrediction.RulLowerBound,
                defaultPrediction.RulUpperBound,
                defaultPrediction.FailureProbability, 
                defaultPrediction.HealthStatus, 
                defaultPrediction.FeatureContributions, 
                defaultPrediction.CreatedAt, 
                defaultPrediction.UpdatedAt,
                defaultPrediction.ModelVersion);
        }

        // Enhanced feature extraction from telemetry
        var features = ExtractFeatures(telemetry);
        
        // Run ML.NET inference with actual feature processing and confidence intervals
        var (rul, lower, upper) = _rulPredictor.PredictWithRange(features);
        var (prob, health) = _healthClassifier.ClassifyWithFeatures(features);

        // Calculate XAI contributions using SHAP (if available) or perturbation-based method
        var modelPath = _mlOptions.Value.RulModelPath;
        var onnxModelPath = modelPath != null && File.Exists(modelPath) 
            ? ConvertToOnnxPath(modelPath) 
            : null;
        
        var contributions = await _xaiService.CalculateContributionsAsync(
            features, 
            f => _rulPredictor.PredictWithFeatures(f),
            rul,
            onnxModelPath);

        var prediction = new Prediction
        {
            Id = Guid.NewGuid(),
            MachineId = machineId,
            RemainingUsefulLifeDays = rul,
            RulLowerBound = lower,
            RulUpperBound = upper,
            FailureProbability = prob,
            HealthStatus = health,
            FeatureContributions = contributions,
            CreatedAt = DateTime.UtcNow,
            ModelVersion = $"v1-{DateTime.UtcNow:yyyyMMdd}"
        };

        await _predictionRepository.AddAsync(prediction);
        await _unitOfWork.SaveChangesAsync();
        await _twinEngineService.UpdateTwinPredictionAsync(machineId, rul, prob, health);
        
        // Trigger alert check
        await _alertService.ProcessPredictionForAlertsAsync(prediction);

        return new PredictionDto(
            prediction.Id, 
            prediction.MachineId, 
            prediction.RemainingUsefulLifeDays, 
            prediction.RulLowerBound,
            prediction.RulUpperBound,
            prediction.FailureProbability, 
            prediction.HealthStatus, 
            prediction.FeatureContributions, 
            prediction.CreatedAt,
            prediction.UpdatedAt,
            prediction.ModelVersion);
    }

    private static Dictionary<string, double> ExtractFeatures(IEnumerable<TelemetryData> telemetry)
    {
        var telemetryList = telemetry.ToList();
        var features = new Dictionary<string, double>();

        // Temperature-based features
        var tempData = telemetryList.Where(t => t.DataType == "temperature").ToList();
        if (tempData.Any())
        {
            var temps = tempData.SelectMany(t =>
            {
                if (TryGetDouble(t.Data, "value", out var temp))
                    return new[] { temp };
                return Array.Empty<double>();
            }).ToList();

            if (temps.Any())
            {
                features[ModelConstants.Features.TemperatureMean] = temps.Average();
                features[ModelConstants.Features.TemperatureStd] = Math.Sqrt(temps.Average(t => Math.Pow(t - features[ModelConstants.Features.TemperatureMean], 2)));
                features["temp_max"] = temps.Max();
                features["temp_min"] = temps.Min();
                features["temp_trend"] = CalculateTrend(temps);
            }
        }

        // Vibration-based features
        var vibData = telemetryList.Where(t => t.DataType == "vibration").ToList();
        if (vibData.Any())
        {
            var vibes = vibData.SelectMany(t =>
            {
                if (TryGetDouble(t.Data, "amplitude", out var vib))
                    return new[] { vib };
                return Array.Empty<double>();
            }).ToList();

            if (vibes.Any())
            {
                features["vib_mean"] = vibes.Average();
                features["vib_std"] = Math.Sqrt(vibes.Average(v => Math.Pow(v - features["vib_mean"], 2)));
                features["vib_max"] = vibes.Max();
                features[ModelConstants.Features.VibrationRms] = Math.Sqrt(vibes.Average(v => v * v));
                features["vib_trend"] = CalculateTrend(vibes);
            }
        }

        // Pressure-based features
        var pressureData = telemetryList.Where(t => t.DataType == "pressure").ToList();
        if (pressureData.Any())
        {
            var pressures = pressureData.SelectMany(t =>
            {
                if (TryGetDouble(t.Data, "value", out var pressure))
                    return new[] { pressure };
                return Array.Empty<double>();
            }).ToList();

            if (pressures.Any())
            {
                features[ModelConstants.Features.PressureMean] = pressures.Average();
                features[ModelConstants.Features.PressureVariance] = Math.Sqrt(pressures.Average(p => Math.Pow(p - features[ModelConstants.Features.PressureMean], 2)));
                features["pressure_trend"] = CalculateTrend(pressures);
            }
        }

        // Time-based features
        features["data_points"] = telemetryList.Count;
        features["time_span_hours"] = telemetryList.Any() ? 
            (telemetryList.Max(t => t.Timestamp) - telemetryList.Min(t => t.Timestamp)).TotalHours : 0;

        return features;
    }

    private static double CalculateTrend(IList<double> values)
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

    private static string? ConvertToOnnxPath(string mlNetModelPath)
    {
        // Convert ML.NET model path to ONNX path
        // In production, this would use an ONNX conversion service
        // For now, return null to use fallback method
        var directory = Path.GetDirectoryName(mlNetModelPath);
        var fileName = Path.GetFileNameWithoutExtension(mlNetModelPath);
        var onnxPath = Path.Combine(directory ?? "models", $"{fileName}.onnx");
        return File.Exists(onnxPath) ? onnxPath : null;
    }

    public async Task<IEnumerable<PredictionDto>> GetHistoryAsync(Guid machineId, int take = 100)
        => (await _predictionRepository.GetAllAsync(p => p.MachineId == machineId))
            .OrderByDescending(p => p.CreatedAt)
            .Take(take)
            .Select(p => new PredictionDto(
                p.Id, 
                p.MachineId, 
                p.RemainingUsefulLifeDays, 
                p.RulLowerBound,
                p.RulUpperBound,
                p.FailureProbability, 
                p.HealthStatus, 
                p.FeatureContributions, 
                p.CreatedAt,
                p.UpdatedAt,
                p.ModelVersion));
}
