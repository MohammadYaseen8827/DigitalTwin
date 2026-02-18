using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.API.Services.Analytics;

public class ModelLifecycleService : IModelLifecycleService
{
    private readonly IModelVersionRepository _modelVersionRepository;
    private readonly IPredictionRepository _predictionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ModelLifecycleService> _logger;

    public ModelLifecycleService(
        IModelVersionRepository modelVersionRepository,
        IPredictionRepository predictionRepository,
        IUnitOfWork unitOfWork,
        ILogger<ModelLifecycleService> logger)
    {
        _modelVersionRepository = modelVersionRepository;
        _predictionRepository = predictionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ModelVersion> RegisterModelVersionAsync(
        string modelType,
        string modelPath,
        Dictionary<string, double> metrics,
        string? trainingDatasetHash = null,
        string? notes = null,
        CancellationToken ct = default)
    {
        var version = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        
        // Check if version already exists
        var existing = await _modelVersionRepository.GetByVersionAsync(modelType, version, ct);
        if (existing != null)
        {
            version = $"{version}-{Guid.NewGuid().ToString("N")[..8]}";
        }

        var modelVersion = new ModelVersion
        {
            Id = Guid.NewGuid(),
            ModelType = modelType,
            Version = version,
            ModelPath = modelPath,
            TrainedAt = DateTime.UtcNow,
            Metrics = metrics,
            TrainingDatasetHash = trainingDatasetHash,
            Status = ModelStatus.Draft,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        await _modelVersionRepository.AddAsync(modelVersion, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Registered new model version: {ModelType} v{Version} with metrics: {Metrics}",
            modelType, version, string.Join(", ", metrics.Select(m => $"{m.Key}={m.Value:F4}")));

        return modelVersion;
    }

    public async Task<ModelVersion> PromoteModelAsync(
        Guid modelVersionId,
        ModelStatus targetStatus,
        string? notes = null,
        CancellationToken ct = default)
    {
        var modelVersion = await _modelVersionRepository.GetAsync(modelVersionId, ct);
        if (modelVersion == null)
        {
            throw new InvalidOperationException($"Model version {modelVersionId} not found");
        }

        // If promoting to Production, deprecate current production version
        if (targetStatus == ModelStatus.Production)
        {
            var currentProduction = await _modelVersionRepository.GetProductionVersionAsync(
                modelVersion.ModelType, ct);
            
            if (currentProduction != null && currentProduction.Id != modelVersionId)
            {
                currentProduction.Status = ModelStatus.Deprecated;
                currentProduction.UpdatedAt = DateTime.UtcNow;
                await _modelVersionRepository.UpdateAsync(currentProduction, ct);
                
                _logger.LogInformation(
                    "Deprecated previous production model: {ModelType} v{Version}",
                    currentProduction.ModelType, currentProduction.Version);
            }
        }

        modelVersion.Status = targetStatus;
        if (targetStatus == ModelStatus.Production)
        {
            modelVersion.PromotedAt = DateTime.UtcNow;
        }
        if (!string.IsNullOrEmpty(notes))
        {
            modelVersion.Notes = notes;
        }
        modelVersion.UpdatedAt = DateTime.UtcNow;

        await _modelVersionRepository.UpdateAsync(modelVersion, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Promoted model {ModelType} v{Version} to {Status}",
            modelVersion.ModelType, modelVersion.Version, targetStatus);

        return modelVersion;
    }

    public async Task<List<ModelVersion>> GetModelVersionsAsync(
        string? modelType = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(modelType))
        {
            return await _modelVersionRepository.GetAllAsync(ct);
        }

        return await _modelVersionRepository.GetByModelTypeAsync(modelType, ct);
    }

    public async Task<ModelVersion?> GetProductionVersionAsync(
        string modelType,
        CancellationToken ct = default)
    {
        return await _modelVersionRepository.GetProductionVersionAsync(modelType, ct);
    }

    public async Task<ModelComparisonResult> CompareModelsAsync(
        Guid modelVersionId1,
        Guid modelVersionId2,
        CancellationToken ct = default)
    {
        var model1 = await _modelVersionRepository.GetAsync(modelVersionId1, ct);
        var model2 = await _modelVersionRepository.GetAsync(modelVersionId2, ct);

        if (model1 == null || model2 == null)
        {
            throw new InvalidOperationException("One or both model versions not found");
        }

        if (model1.ModelType != model2.ModelType)
        {
            throw new InvalidOperationException("Cannot compare models of different types");
        }

        var comparison = new ModelComparisonResult
        {
            Model1 = model1,
            Model2 = model2,
            Metrics = new Dictionary<string, ComparisonMetric>()
        };

        // Compare all metrics
        var allMetricKeys = model1.Metrics.Keys.Union(model2.Metrics.Keys).Distinct();
        var model1Wins = 0;
        var model2Wins = 0;

        foreach (var key in allMetricKeys)
        {
            var value1 = model1.Metrics.GetValueOrDefault(key, 0);
            var value2 = model2.Metrics.GetValueOrDefault(key, 0);
            
            // Determine if higher or lower is better based on metric name
            var isHigherBetter = key.ToLower().Contains("r2") || 
                                 key.ToLower().Contains("accuracy") ||
                                 key.ToLower().Contains("score");
            
            var difference = value1 - value2;
            var differencePercentage = value2 != 0 
                ? Math.Abs(difference / value2) * 100 
                : 0;

            var isBetter = isHigherBetter 
                ? value1 > value2 
                : value1 < value2;

            if (isBetter) model1Wins++;
            else model2Wins++;

            comparison.Metrics[key] = new ComparisonMetric
            {
                Name = key,
                Value1 = value1,
                Value2 = value2,
                Difference = difference,
                DifferencePercentage = differencePercentage,
                IsBetter = isBetter,
                BetterModel = isBetter ? model1.Version : model2.Version
            };
        }

        comparison.Winner = model1Wins > model2Wins 
            ? model1.Version 
            : model2Wins > model1Wins 
                ? model2.Version 
                : "Tie";

        // Generate differences summary
        comparison.Differences = comparison.Metrics.Values
            .Where(m => Math.Abs(m.DifferencePercentage) > 1.0) // More than 1% difference
            .Select(m => $"{m.Name}: {m.BetterModel} is {Math.Abs(m.DifferencePercentage):F2}% better")
            .ToList();

        return comparison;
    }

    public async Task<ModelPerformanceSummary> GetModelPerformanceAsync(
        Guid modelVersionId,
        CancellationToken ct = default)
    {
        var modelVersion = await _modelVersionRepository.GetAsync(modelVersionId, ct);
        if (modelVersion == null)
        {
            throw new InvalidOperationException($"Model version {modelVersionId} not found");
        }

        // Get predictions using this model version
        var predictions = (await _predictionRepository.GetAllAsync(
            p => p.ModelVersion == modelVersion.Version, ct)).ToList();

        var summary = new ModelPerformanceSummary
        {
            ModelVersion = modelVersion,
            TotalPredictions = predictions.Count,
            PerformanceMetrics = new Dictionary<string, double>()
        };

        if (predictions.Any())
        {
            // Calculate average RUL error (if we had ground truth)
            // For now, use variance as a proxy
            var rulValues = predictions.Select(p => p.RemainingUsefulLifeDays).ToList();
            var avgRul = rulValues.Average();
            var rulVariance = rulValues.Average(v => Math.Pow(v - avgRul, 2));
            
            summary.AverageRulError = Math.Sqrt(rulVariance);
            summary.AverageFailureProbability = predictions.Average(p => p.FailureProbability);
            summary.LastUsedAt = predictions.Max(p => p.CreatedAt);

            // Add model metrics to performance summary
            foreach (var metric in modelVersion.Metrics)
            {
                summary.PerformanceMetrics[metric.Key] = metric.Value;
            }
        }

        return summary;
    }
}
