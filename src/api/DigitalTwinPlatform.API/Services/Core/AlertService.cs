using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Alerts.Models;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.API.Services.Infrastructure;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.API.Services.Core;

/// <summary>
/// Service interface for managing system alerts.
/// </summary>
public interface IAlertService
{
    /// <summary>
    /// Processes prediction results and creates alerts if thresholds are exceeded.
    /// </summary>
    /// <param name="prediction">The prediction result to process.</param>
    Task ProcessPredictionForAlertsAsync(Prediction prediction);

    /// <summary>
    /// Retrieves active (unacknowledged) alerts, optionally filtered by machine.
    /// </summary>
    /// <param name="machineId">Optional machine ID filter.</param>
    /// <returns>Collection of active alerts.</returns>
    Task<IEnumerable<Alert>> GetActiveAlertsAsync(Guid? machineId = null);

    /// <summary>
    /// Retrieves all alerts including acknowledged ones, optionally filtered by machine.
    /// </summary>
    /// <param name="machineId">Optional machine ID filter.</param>
    /// <returns>Collection of all alerts.</returns>
    Task<IEnumerable<Alert>> GetAllAlertsAsync(Guid? machineId = null);

    /// <summary>
    /// Retrieves a specific alert by its ID.
    /// </summary>
    /// <param name="alertId">The unique alert identifier.</param>
    /// <returns>The alert or null if not found.</returns>
    Task<Alert?> GetAlertByIdAsync(Guid alertId);

    /// <summary>
    /// Acknowledges an alert, marking it as seen by a user.
    /// </summary>
    /// <param name="alertId">The unique alert identifier.</param>
    /// <param name="userId">The ID of the user acknowledging the alert.</param>
    Task AcknowledgeAlertAsync(Guid alertId, string userId);

    /// <summary>
    /// Resolves (deletes) an alert from the system.
    /// </summary>
    /// <param name="alertId">The unique alert identifier.</param>
    Task ResolveAlertAsync(Guid alertId);

    /// <summary>
    /// Searches alerts based on query and filters.
    /// </summary>
    /// <param name="query">Text query to search in alert fields.</param>
    /// <param name="status">Optional status filter (active, acknowledged, resolved).</param>
    /// <param name="severity">Optional severity filter (info, warning, critical, error).</param>
    /// <param name="machineId">Optional machine ID filter.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>Collection of alerts matching the criteria.</returns>
    Task<IEnumerable<Alert>> SearchAlertsAsync(string query, string? status = null, string? severity = null, Guid? machineId = null, CancellationToken ct = default);

    /// <summary>
    /// Gets alert statistics for dashboard display.
    /// </summary>
    /// <returns>Statistics including counts by severity and status.</returns>
    Task<AlertStats> GetAlertStatsAsync();
}

/// <summary>
/// Statistics for alert dashboard display.
/// </summary>
public record AlertStats(
    int TotalActive,
    int TotalAcknowledged,
    int TotalResolved,
    int CriticalCount,
    int WarningCount,
    int InfoCount
);

public class AlertService : IAlertService
{
    private readonly IRepository<Alert> _alertRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubPublisher _hubPublisher;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AlertService> _logger;

    public AlertService(
        IRepository<Alert> alertRepository,
        IUnitOfWork unitOfWork,
        IHubPublisher hubPublisher,
        INotificationService notificationService,
        ILogger<AlertService> logger)
    {
        _alertRepository = alertRepository;
        _unitOfWork = unitOfWork;
        _hubPublisher = hubPublisher;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ProcessPredictionForAlertsAsync(Prediction prediction)
    {
        // RUL Threshold check (Critical: < 7 days, Warning: < 30 days)
        if (prediction.RemainingUsefulLifeDays < 7)
        {
            await CreateAlertAsync(prediction.MachineId, $"CRITICAL: Remaining Useful Life is extremely low ({prediction.RemainingUsefulLifeDays:F1} days)", AlertSeverity.Critical);
        }
        else if (prediction.RemainingUsefulLifeDays < 30)
        {
            await CreateAlertAsync(prediction.MachineId, $"WARNING: Remaining Useful Life is below 30 days ({prediction.RemainingUsefulLifeDays:F1} days)", AlertSeverity.Warning);
        }

        // Failure Probability check
        if (prediction.FailureProbability > 0.8)
        {
            await CreateAlertAsync(prediction.MachineId, $"CRITICAL: High failure probability detected ({prediction.FailureProbability:P1})", AlertSeverity.Critical);
        }
    }

    private async Task CreateAlertAsync(Guid machineId, string message, DigitalTwinPlatform.Domain.Entities.AlertSeverity severity)
    {
        // Avoid duplicate active alerts for the same machine with the same message
        var existing = await _alertRepository.GetAllAsync(a => a.MachineId == machineId && a.Message == message && !a.IsAcknowledged);
        if (existing.Any()) return;

        var alert = new Alert
        {
            MachineId = machineId,
            Message = message,
            Severity = severity,
            CreatedAt = DateTime.UtcNow,
            IsAcknowledged = false
        };

        await _alertRepository.AddAsync(alert);
        await _unitOfWork.SaveChangesAsync();
        
        // Broadcast to SignalR
        await _hubPublisher.BroadcastAlertAsync(machineId, new AlertDto(alert.Id, alert.MachineId, alert.Message, alert.Severity, alert.CreatedAt, alert.IsAcknowledged));
        
        // Dispatch notifications for Critical and Warning alerts
        if (severity >= DigitalTwinPlatform.Domain.Entities.AlertSeverity.Warning)
        {
            var channels = new List<string> { "email", "webhook" };
            await _notificationService.SendNotificationAsync(alert, channels);
        }

        _logger.LogInformation("Alert created for machine {MachineId}: {Message}", machineId, message);
    }

    public async Task<IEnumerable<Alert>> GetActiveAlertsAsync(Guid? machineId = null)
    {
        if (machineId.HasValue)
        {
            return await _alertRepository.GetAllAsync(a => a.MachineId == machineId.Value && !a.IsAcknowledged);
        }
        return await _alertRepository.GetAllAsync(a => !a.IsAcknowledged);
    }

    public async Task<IEnumerable<Alert>> GetAllAlertsAsync(Guid? machineId = null)
    {
        if (machineId.HasValue)
        {
            return await _alertRepository.GetAllAsync(a => a.MachineId == machineId.Value);
        }
        return await _alertRepository.GetAllAsync();
    }

    public async Task<Alert?> GetAlertByIdAsync(Guid alertId)
    {
        return await _alertRepository.GetAsync(alertId);
    }

    public async Task AcknowledgeAlertAsync(Guid alertId, string userId)
    {
        var alert = await _alertRepository.GetAsync(alertId);
        if (alert != null)
        {
            alert.Acknowledge(userId); // Use the domain method to properly acknowledge the alert
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task ResolveAlertAsync(Guid alertId)
    {
        var alert = await _alertRepository.GetAsync(alertId);
        if (alert != null)
        {
            alert.Resolve(); // Use the domain method to properly resolve the alert
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Alert>> SearchAlertsAsync(string query, string? status = null, string? severity = null, Guid? machineId = null, CancellationToken ct = default)
    {
        // Use the repository's search method
        var alerts = await _alertRepository.SearchAsync(query, status, severity, ct);
        
        // Apply additional filters if specified
        if (machineId.HasValue)
        {
            alerts = alerts.Where(a => a.MachineId == machineId.Value);
        }
        
        return alerts;
    }

    public async Task<AlertStats> GetAlertStatsAsync()
    {
        var allAlerts = await _alertRepository.GetAllAsync();
        
        return new AlertStats(
            TotalActive: allAlerts.Count(a => a.Status == "active"),
            TotalAcknowledged: allAlerts.Count(a => a.Status == "acknowledged"),
            TotalResolved: allAlerts.Count(a => a.Status == "resolved"),
            CriticalCount: allAlerts.Count(a => a.Severity == AlertSeverity.Critical),
            WarningCount: allAlerts.Count(a => a.Severity == AlertSeverity.Warning),
            InfoCount: allAlerts.Count(a => a.Severity == AlertSeverity.Info)
        );
    }
}
