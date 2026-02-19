using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.Application.Alerts.Models;

public record AlertDto(
    Guid Id,
    Guid MachineId,
    string Message,
    string Title,
    string Description,
    AlertSeverity Severity,
    string Status,
    DateTime CreatedAt,
    DateTime? AcknowledgedAt = null,
    DateTime? ResolvedAt = null,
    bool IsAcknowledged = false,
    string? AcknowledgedBy = null,
    Guid? RelatedPredictionId = null,
    string? Category = null,
    string? RecommendedAction = null,
    string? SuggestedActions = null)
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
            alert.Title,
            alert.Description,
            alert.Severity,
            alert.Status,
            alert.CreatedAt,
            alert.AcknowledgedAt,
            alert.ResolvedAt,
            alert.IsAcknowledged,
            alert.AcknowledgedBy,
            alert.RelatedPredictionId,
            alert.Category,
            alert.RecommendedAction,
            alert.SuggestedActions
        );
    }
}
