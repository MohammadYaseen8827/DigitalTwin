using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Application.Services;
using DigitalTwinPlatform.Application.Abstractions.Services;

namespace DigitalTwinPlatform.API.Hubs;

/// <summary>
/// SignalR hub for real-time analytics streaming.
/// Provides predictions, alerts, and system health updates.
/// </summary>
public class RealTimeAnalyticsHub : Hub<IAnalyticsClient>
{
    private readonly ILogger<RealTimeAnalyticsHub> _logger;
    private readonly IPredictionService _predictionService;

    public RealTimeAnalyticsHub(
        ILogger<RealTimeAnalyticsHub> logger,
        IPredictionService predictionService)
    {
        _logger = logger;
        _predictionService = predictionService;
    }

    /// <summary>
    /// Gets the user ID from the connection context.
    /// </summary>
    private string? GetUserId() => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? Context.User?.FindFirst("sub")?.Value;

    /// <summary>
    /// Subscribe to prediction updates for a specific machine.
    /// </summary>
    /// <param name="machineId">The machine ID for predictions.</param>
    public async Task SubscribeToPredictions(string machineId)
    {
        var userId = GetUserId();
        var connectionId = Context.ConnectionId;

        _logger.LogInformation("User {UserId} subscribing to predictions for machine {MachineId}",
            userId ?? "anonymous", machineId);

        await Groups.AddToGroupAsync(connectionId, $"predictions-{machineId}");

        await Clients.Caller.PredictionUpdate(new PredictionData
        {
            MachineId = machineId,
            Timestamp = DateTime.UtcNow,
            Message = $"Subscribed to prediction updates for machine {machineId}"
        });

        _logger.LogDebug("Client {ConnectionId} subscribed to predictions for {MachineId}", connectionId, machineId);
    }

    /// <summary>
    /// Unsubscribe from prediction updates.
    /// </summary>
    public async Task UnsubscribeFromPredictions(string machineId)
    {
        var userId = GetUserId();
        var connectionId = Context.ConnectionId;

        _logger.LogInformation("User {UserId} unsubscribing from predictions for machine {MachineId}",
            userId ?? "anonymous", machineId);

        await Groups.RemoveFromGroupAsync(connectionId, $"predictions-{machineId}");

        await Clients.Caller.PredictionUpdate(new PredictionData
        {
            MachineId = machineId,
            Timestamp = DateTime.UtcNow,
            Message = $"Unsubscribed from prediction updates for machine {machineId}"
        });
    }

    /// <summary>
    /// Subscribe to alerts stream.
    /// </summary>
    public async Task SubscribeToAlerts()
    {
        var userId = GetUserId();
        var connectionId = Context.ConnectionId;

        _logger.LogInformation("User {UserId} subscribing to alerts stream", userId ?? "anonymous");

        await Groups.AddToGroupAsync(connectionId, "alerts");

        await Clients.Caller.NewAlert(new AlertData
        {
            AlertId = Guid.NewGuid().ToString(),
            MachineId = "system",
            Timestamp = DateTime.UtcNow,
            Severity = AlertSeverity.Info,
            Message = "Successfully subscribed to alerts stream"
        });

        _logger.LogDebug("Client {ConnectionId} subscribed to alerts", connectionId);
    }

    /// <summary>
    /// Unsubscribe from alerts stream.
    /// </summary>
    public async Task UnsubscribeFromAlerts()
    {
        var userId = GetUserId();
        var connectionId = Context.ConnectionId;

        _logger.LogInformation("User {UserId} unsubscribing from alerts stream", userId ?? "anonymous");

        await Groups.RemoveFromGroupAsync(connectionId, "alerts");
    }

    /// <summary>
    /// Subscribe to system health metrics.
    /// </summary>
    public async Task SubscribeToSystemHealth()
    {
        var userId = GetUserId();
        _logger.LogInformation("User {UserId} subscribing to system health stream", userId ?? "anonymous");

        await Groups.AddToGroupAsync(Context.ConnectionId, "system-health");
    }

    /// <summary>
    /// Unsubscribe from system health metrics.
    /// </summary>
    public async Task UnsubscribeFromSystemHealth()
    {
        var userId = GetUserId();
        _logger.LogInformation("User {UserId} unsubscribing from system health stream", userId ?? "anonymous");

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "system-health");
    }

    /// <summary>
    /// Subscribe to all analytics streams for a machine.
    /// </summary>
    public async Task SubscribeToAllAnalytics(string machineId)
    {
        var userId = GetUserId();
        _logger.LogInformation("User {UserId} subscribing to all analytics for machine {MachineId}",
            userId ?? "anonymous", machineId);

        await Groups.AddToGroupAsync(Context.ConnectionId, $"telemetry-{machineId}");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"predictions-{machineId}");
        await Groups.AddToGroupAsync(Context.ConnectionId, "alerts");
        await Groups.AddToGroupAsync(Context.ConnectionId, "system-health");

        await Clients.Caller.AnalyticsConnected(new AnalyticsConnectionData
        {
            MachineId = machineId,
            ConnectedAt = DateTime.UtcNow,
            Streams = new[] { "telemetry", "predictions", "alerts", "system-health" }
        });
    }

    /// <summary>
    /// Unsubscribe from all analytics streams.
    /// </summary>
    public async Task UnsubscribeFromAllAnalytics(string machineId)
    {
        var userId = GetUserId();
        _logger.LogInformation("User {UserId} unsubscribing from all analytics for machine {MachineId}",
            userId ?? "anonymous", machineId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"telemetry-{machineId}");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"predictions-{machineId}");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "alerts");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "system-health");
    }

    /// <summary>
    /// Handle client connection.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        _logger.LogInformation("Analytics client connected: {ConnectionId} (User: {UserId})",
            Context.ConnectionId, userId ?? "anonymous");

        await Clients.Caller.AnalyticsConnected(new AnalyticsConnectionData
        {
            ConnectionId = Context.ConnectionId,
            ConnectedAt = DateTime.UtcNow,
            Message = "Connected to Real-Time Analytics Hub"
        });

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Broadcast new prediction to all subscribers.
    /// </summary>
    public async Task BroadcastPredictionAsync(Guid machineId)
    {
        try
        {
            var prediction = await _predictionService.GetLatestPredictionAsync(machineId);
            if (prediction != null)
            {
                await Clients.Group($"predictions-{machineId}").PredictionUpdate(new PredictionData
                {
                    MachineId = machineId.ToString(),
                    Timestamp = DateTime.UtcNow,
                    RulRemaining = prediction.RemainingUsefulLifeDays,
                    FailureProbability = prediction.FailureProbability,
                    ConfidenceLevel = null,
                    PredictionType = prediction.HealthStatus.ToString(),
                    FeatureImportance = prediction.FeatureContributions,
                    Message = "New prediction available"
                });

                _logger.LogDebug("Broadcasted prediction for machine {MachineId}", machineId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast prediction for machine {MachineId}", machineId);
        }
    }

    /// <summary>
    /// Handle client disconnection with proper cleanup.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var connectionId = Context.ConnectionId;

        if (exception != null)
        {
            _logger.LogWarning(exception, "Analytics client {ConnectionId} disconnected with error (User: {UserId})",
                connectionId, userId ?? "anonymous");
        }
        else
        {
            _logger.LogInformation("Analytics client disconnected: {ConnectionId} (User: {UserId})",
                connectionId, userId ?? "anonymous");
        }

        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// Client interface for RealTimeAnalyticsHub methods.
/// </summary>
public interface IAnalyticsClient
{
    /// <summary>
    /// Receive prediction update.
    /// </summary>
    Task PredictionUpdate(PredictionData data);

    /// <summary>
    /// Receive new alert.
    /// </summary>
    Task NewAlert(AlertData data);

    /// <summary>
    /// Analytics connection confirmed.
    /// </summary>
    Task AnalyticsConnected(AnalyticsConnectionData data);
}

/// <summary>
/// Prediction data model.
/// </summary>
public class PredictionData
{
    public string MachineId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double? RulRemaining { get; set; }
    public double? FailureProbability { get; set; }
    public double? ConfidenceLevel { get; set; }
    public string? PredictionType { get; set; }
    public Dictionary<string, double>? FeatureImportance { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Alert data model.
/// </summary>
public class AlertData
{
    public string AlertId { get; set; } = Guid.NewGuid().ToString();
    public string MachineId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Category { get; set; } = "General";
    public string Message { get; set; } = string.Empty;
    public string? RecommendedAction { get; set; }
    public bool Acknowledged { get; set; }
}


/// <summary>
/// Analytics connection data.
/// </summary>
public class AnalyticsConnectionData
{
    public string? ConnectionId { get; set; }
    public string? MachineId { get; set; }
    public DateTime ConnectedAt { get; set; }
    public string[]? Streams { get; set; }
    public string? Message { get; set; }
}
