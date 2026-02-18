using DigitalTwinPlatform.Application.Predictions.Models;
using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.Application.Predictions;

internal static class PredictionMapper
{
    public static PredictionDto ToDto(Prediction prediction) => new(
        prediction.Id,
        prediction.MachineId,
        prediction.RemainingUsefulLifeDays,
        prediction.RulLowerBound,
        prediction.RulUpperBound,
        prediction.FailureProbability,
        prediction.HealthStatus,
        prediction.FeatureContributions,
        prediction.CreatedAt,
        prediction.ModelVersion);
}
