using DigitalTwinPlatform.Domain.Entities.Enums;
using DigitalTwinPlatform.Domain.Enums;

namespace DigitalTwinPlatform.Application.Predictions.Models;

public record PredictionDto(
    Guid Id,
    Guid MachineId,
    double RemainingUsefulLifeDays,
    double RulLowerBound,
    double RulUpperBound,
    double FailureProbability,
    HealthClassification HealthStatus,
    Dictionary<string, double>? FeatureContributions,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string ModelVersion);

public record PredictionRequestDto(Guid MachineId);
