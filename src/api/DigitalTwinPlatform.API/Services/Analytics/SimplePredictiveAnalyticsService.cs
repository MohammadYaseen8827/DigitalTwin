using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Predictions.Models;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Entities.Enums;
using DigitalTwinPlatform.Domain.Enums;

namespace DigitalTwinPlatform.API.Services.Analytics;

/// <summary>
/// Simplified predictive analytics service without ML dependencies.
/// Provides basic predictions based on telemetry statistics.
/// </summary>
public class SimplePredictiveAnalyticsService : IPredictiveAnalyticsService
{
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly IRepository<Prediction> _predictionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SimplePredictiveAnalyticsService(
        ITelemetryRepository telemetryRepository,
        IRepository<Prediction> predictionRepository,
        IUnitOfWork unitOfWork)
    {
        _telemetryRepository = telemetryRepository;
        _predictionRepository = predictionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PredictionDto> PredictAsync(Guid machineId)
    {
        // Fetch recent telemetry
        var telemetry = (await _telemetryRepository.GetForMachineAsync(machineId, null, 50)).ToList();
        
        double rul;
        double failureProbability;
        HealthClassification healthStatus;

        if (!telemetry.Any())
        {
            // Default prediction if no telemetry available
            rul = 365.0;
            failureProbability = 0.1;
            healthStatus = HealthClassification.Healthy;
        }
        else
        {
            // Simple heuristic-based prediction
            var recentTelemetry = telemetry.OrderByDescending(t => t.Timestamp).Take(10).ToList();
            
            // Calculate simple health score based on data recency and count
            var hoursSinceLastReading = (DateTime.UtcNow - recentTelemetry.First().Timestamp).TotalHours;
            var dataPointsLast24h = telemetry.Count(t => t.Timestamp > DateTime.UtcNow.AddHours(-24));
            
            // Simple scoring logic
            if (hoursSinceLastReading > 24 || dataPointsLast24h < 5)
            {
                healthStatus = HealthClassification.FailureImminent;
                failureProbability = 0.8;
                rul = 7.0; // 7 days
            }
            else if (hoursSinceLastReading > 12 || dataPointsLast24h < 10)
            {
                healthStatus = HealthClassification.SignificantDegradation;
                failureProbability = 0.4;
                rul = 30.0; // 30 days
            }
            else if (hoursSinceLastReading > 6 || dataPointsLast24h < 20)
            {
                healthStatus = HealthClassification.MinorDegradation;
                failureProbability = 0.2;
                rul = 90.0; // 90 days
            }
            else
            {
                healthStatus = HealthClassification.Healthy;
                failureProbability = 0.05;
                rul = 180.0; // 180 days
            }
        }

        var prediction = new Prediction
        {
            Id = Guid.NewGuid(),
            MachineId = machineId,
            RemainingUsefulLifeDays = rul,
            FailureProbability = failureProbability,
            HealthStatus = healthStatus,
            CreatedAt = DateTime.UtcNow,
            ModelVersion = "v1-simple-heuristic"
        };

        await _predictionRepository.AddAsync(prediction);
        await _unitOfWork.SaveChangesAsync();

        return new PredictionDto(
            prediction.Id,
            prediction.MachineId,
            prediction.RemainingUsefulLifeDays,
            prediction.RemainingUsefulLifeDays * 0.9, // Lower bound estimate
            prediction.RemainingUsefulLifeDays * 1.1, // Upper bound estimate
            prediction.FailureProbability,
            prediction.HealthStatus,
            null, // No feature contributions for heuristic
            prediction.CreatedAt,
            prediction.UpdatedAt,
            prediction.ModelVersion);
    }

    public async Task<IEnumerable<PredictionDto>> GetHistoryAsync(Guid machineId, int take = 100)
    {
        var predictions = await _predictionRepository.GetAllAsync(p => p.MachineId == machineId);
        
        return predictions
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
}
