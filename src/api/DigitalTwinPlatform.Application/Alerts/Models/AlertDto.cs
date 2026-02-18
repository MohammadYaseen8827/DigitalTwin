using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.Application.Alerts.Models;

public record AlertDto(
    Guid Id,
    Guid MachineId,
    string Message,
    AlertSeverity Severity,
    DateTime CreatedAt,
    bool IsAcknowledged,
    string? AcknowledgedBy = null,
    Guid? RelatedPredictionId = null,
    string? Category = null,
    string? RecommendedAction = null)
{
    /// <summary>
    /// Creates an AlertDto from a domain Alert entity.
    /// </summary>
    public static AlertDto FromEntity(Alert alert)
    {
        return new AlertDto(
            alert.Id,
            alert.MachineId,
            alert.Message,
            alert.Severity,
            alert.CreatedAt,
            alert.IsAcknowledged,
            alert.AcknowledgedBy,
            alert.RelatedPredictionId);
    }
}
