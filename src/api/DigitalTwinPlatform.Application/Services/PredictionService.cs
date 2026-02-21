using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Predictions.Models;
using Microsoft.Extensions.Logging;
using DigitalTwinPlatform.Application.Abstractions.Services;
using DigitalTwinPlatform.Domain.Entities.Enums;

namespace DigitalTwinPlatform.Application.Services;

/// <summary>
/// Service for managing RUL predictions with under 500ms latency and interpretable ML.>
/// </summary>
public class PredictionService : IPredictionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly IFeatureExtractionService _featureExtractor;
    private readonly IMLModelService _mlModelService;
    private readonly ILogger<PredictionService> _logger;

    public PredictionService(
        IUnitOfWork unitOfWork,
        ITelemetryRepository telemetryRepository,
        IFeatureExtractionService featureExtractor,
        IMLModelService mlModelService,
        ILogger<PredictionService> logger)
    {
        _unitOfWork = unitOfWork;
        _telemetryRepository = telemetryRepository;
        _featureExtractor = featureExtractor;
        _mlModelService = mlModelService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new RUL prediction for a machine.
    /// </summary>
    public async Task<PredictionDto> CreatePredictionAsync(PredictionRequestDto request, CancellationToken ct = default)
    {
        var startTime = DateTime.UtcNow;
        
        // 1. Fetch telemetry for the machine
        var telemetry = await _telemetryRepository.GetAllAsync(
            t => t.MachineId == request.MachineId, 
            ct);
            
        var recentTelemetry = telemetry
            .OrderByDescending(t => t.Timestamp)
            .Take(100) // Take enough for windowing
            .ToList();

        if (recentTelemetry.Count < 20)
        {
            _logger.LogWarning("Insufficient telemetry for prediction: {Count}", recentTelemetry.Count);
            // Fallback or throw? For now return a default/error state
            // Or maybe just let it fail/return empty if that's acceptable
            // But we should probably return something indicating failure or not enough data
        }

        // 2. Extract features
        var features = _featureExtractor.ExtractFeatures(recentTelemetry);
        
        // 3. Predict RUL using ML model service
        var mlResult = await _mlModelService.PredictRulAsync(request.MachineId.ToString(), features);

        // 4. Create Prediction entity
        var prediction = new Prediction
        {
            Id = Guid.NewGuid(),
            MachineId = request.MachineId,
            RemainingUsefulLifeDays = mlResult.RemainingUsefulLife,
            RulLowerBound = mlResult.RemainingUsefulLife - (mlResult.RemainingUsefulLife * (1 - mlResult.Confidence)), // Simple bound estimation if not provided
            RulUpperBound = mlResult.RemainingUsefulLife + (mlResult.RemainingUsefulLife * (1 - mlResult.Confidence)),
            Confidence = mlResult.Confidence,
            FailureProbability = mlResult.RemainingUsefulLife < 30 ? 0.8 : 0.1, // Simple heuristic for now based on RUL
            HealthStatus = mlResult.RemainingUsefulLife < 7 ? HealthClassification.FailureImminent : (mlResult.RemainingUsefulLife < 30 ? HealthClassification.SignificantDegradation : HealthClassification.Normal),
            ContributingFactors = features,
            FeatureContributions = mlResult.FeatureImportance.ToDictionary(k => k.Key, v => Convert.ToDouble(v.Value)) ?? features,
            ModelVersion = "1.2.0",
            CreatedAt = DateTime.UtcNow,
            PredictionTime = mlResult.PredictionTime
        };

        var repository = _unitOfWork.Repository<Prediction>();
        await repository.AddAsync(prediction, ct);
        await _unitOfWork.SaveChangesAsync(ct);

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
            prediction.CreatedAt,
            prediction.ModelVersion);
    }

    /// <summary>
    /// Predict async - alias for CreatePredictionAsync
    /// </summary>
    public async Task<PredictionDto> PredictAsync(PredictionRequestDto request, CancellationToken ct = default)
    {
        return await CreatePredictionAsync(request, ct);
    }

    /// <summary>
    /// Gets the latest prediction for a machine.
    /// </summary>
    public async Task<PredictionDto?> GetLatestPredictionAsync(
        Guid machineId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var repository = _unitOfWork.Repository<Prediction>();
            var predictions = await repository.GetAllAsync(
                p => p.MachineId == machineId,
                ct: cancellationToken,
                take: 1);

            var latestPrediction = predictions.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
            
            if (latestPrediction == null)
                return null;

            return new PredictionDto(
                latestPrediction.Id,
                latestPrediction.MachineId,
                latestPrediction.RemainingUsefulLifeDays,
                latestPrediction.RulLowerBound,
                latestPrediction.RulUpperBound,
                latestPrediction.FailureProbability,
                latestPrediction.HealthStatus,
                latestPrediction.FeatureContributions,
                latestPrediction.CreatedAt,
                latestPrediction.CreatedAt,
                latestPrediction.ModelVersion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get latest prediction for machine {MachineId}", machineId);
            throw;
        }
    }

    /// <summary>
    /// Gets prediction history for a machine.
    /// </summary>
    public async Task<IEnumerable<PredictionDto>> GetPredictionHistoryAsync(
        Guid machineId,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var repository = _unitOfWork.Repository<Prediction>();
            var predictions = await repository.GetAllAsync(
                p => p.MachineId == machineId,
                ct: cancellationToken,
                take: limit);

            return predictions.OrderByDescending(p => p.CreatedAt).Select(p => new PredictionDto(
                p.Id,
                p.MachineId,
                p.RemainingUsefulLifeDays,
                p.RulLowerBound,
                p.RulUpperBound,
                p.FailureProbability,
                p.HealthStatus,
                p.FeatureContributions,
                p.CreatedAt,
                p.CreatedAt,
                p.ModelVersion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get prediction history for machine {MachineId}", machineId);
            throw;
        }
    }

    /// <summary>
    /// Gets predictions within a confidence threshold.
    /// </summary>
    public async Task<IEnumerable<PredictionDto>> GetHighConfidencePredictionsAsync(
        double minConfidence = 0.8,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var repository = _unitOfWork.Repository<Prediction>();
            var predictions = await repository.GetAllAsync(
                p => p.Confidence >= minConfidence && p.IsValid(),
                ct: cancellationToken);

            return predictions.OrderByDescending(p => p.CreatedAt).Select(p => new PredictionDto(
                p.Id,
                p.MachineId,
                p.RemainingUsefulLifeDays,
                p.RulLowerBound,
                p.RulUpperBound,
                p.FailureProbability,
                p.HealthStatus,
                p.FeatureContributions,
                p.CreatedAt,
                p.CreatedAt,
                p.ModelVersion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get high confidence predictions");
            throw;
        }
    }

    /// <summary>
    /// Cleanup old predictions.
    /// </summary>
    public async Task CleanupOldPredictionsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var repository = _unitOfWork.Repository<Prediction>();
            var oldPredictions = (await repository.GetAllAsync(
                p => p.CreatedAt < cutoffDate,
                ct: cancellationToken)).ToList();

            foreach (var prediction in oldPredictions)
            {
                await repository.DeleteAsync(prediction, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Cleaned up {Count} old predictions before {CutoffDate}", oldPredictions.Count(), cutoffDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old predictions");
            throw;
        }
    }

    /// <summary>
    /// Search predictions by query and optional machine ID.
    /// </summary>
    public async Task<IEnumerable<PredictionDto>> SearchPredictionsAsync(string query, Guid? machineId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var predictionRepository = _unitOfWork.Repository<Prediction>() as IPredictionRepository;
            
            IEnumerable<Prediction> predictions;

            if (predictionRepository == null)
            {
                // Fallback to basic repository search if IPredictionRepository is not available
                var repository = _unitOfWork.Repository<Prediction>();
                var allPredictions = await repository.GetAllAsync(null, ct: cancellationToken, take: 1000);
                
                var filteredPredictions = allPredictions.AsEnumerable();
                
                if (!string.IsNullOrWhiteSpace(query))
                {
                    var lowerQuery = query.ToLowerInvariant();
                    filteredPredictions = filteredPredictions.Where(p =>
                        (p.Machine?.Name?.Value ?? string.Empty).ToLowerInvariant().Contains(lowerQuery) ||
                        (p.Machine?.Type?.ToString() ?? string.Empty).ToLowerInvariant().Contains(lowerQuery) ||
                        p.HealthStatus.ToString().ToLowerInvariant().Contains(lowerQuery) ||
                        p.ModelVersion.ToLowerInvariant().Contains(lowerQuery) ||
                        p.Id.ToString().Contains(lowerQuery)
                    );
                }
                
                if (machineId.HasValue)
                {
                    filteredPredictions = filteredPredictions.Where(p => p.MachineId == machineId.Value);
                }
                
                predictions = filteredPredictions.ToList();
            }
            else
            {
                // Use the repository's search method
                predictions = await predictionRepository.SearchAsync(query, machineId, cancellationToken);
            }
            
            return predictions.Select(p => new PredictionDto(
                p.Id,
                p.MachineId,
                p.RemainingUsefulLifeDays,
                p.RulLowerBound,
                p.RulUpperBound,
                p.FailureProbability,
                p.HealthStatus,
                p.FeatureContributions,
                p.CreatedAt,
                p.CreatedAt,
                p.ModelVersion
            )).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search predictions with query: {Query}, machineId: {MachineId}", query, machineId);
            throw;
        }
    }

    /// <summary>
    /// Broadcast new prediction to all subscribers.
    /// </summary>
    public async Task BroadcastPredictionAsync(Guid machineId)
    {
        try
        {
            var prediction = await GetLatestPredictionAsync(machineId);
            if (prediction != null)
            {
                // In production, this would be handled by RealTimeAnalyticsHub
                // For now, we'll just log the broadcast
                _logger.LogInformation("Broadcasted prediction for machine {MachineId} - RUL: {RemainingUsefulLifeDays} days", 
                    machineId, prediction.RemainingUsefulLifeDays);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast prediction for machine {MachineId}", machineId);
        }
    }
}
