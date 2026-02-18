using DigitalTwinPlatform.Application.Predictions.Models;
using DigitalTwinPlatform.Application.Alerts.Models;
using DigitalTwinPlatform.API.Hubs;
using DigitalTwinPlatform.Application.Telemetry.Models;
using Microsoft.AspNetCore.SignalR;
using TelemetryData = DigitalTwinPlatform.API.Hubs.TelemetryData;

namespace DigitalTwinPlatform.API.Services.Infrastructure;


/// <summary>
/// Central service for publishing messages to SignalR hubs.
/// Enables real-time communication from anywhere in the application.
/// </summary>
public class HubPublisher : IHubPublisher
{
    private readonly IHubContext<TelemetryHub> _telemetryHubContext;
    private readonly IHubContext<RealTimeAnalyticsHub> _analyticsHubContext;
    private readonly ILogger<HubPublisher> _logger;

    public HubPublisher(
        IHubContext<TelemetryHub> telemetryHubContext,
        IHubContext<RealTimeAnalyticsHub> analyticsHubContext,
        ILogger<HubPublisher> logger)
    {
        _telemetryHubContext = telemetryHubContext;
        _analyticsHubContext = analyticsHubContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task BroadcastTelemetryAsync(Guid machineId, TelemetryDto telemetry)
    {
        _logger.LogDebug("Broadcasting telemetry for machine {MachineId}", machineId);

        // Use the hubs.TelemetryData type for SignalR communication
        await _telemetryHubContext.Clients
            .Group($"telemetry-{machineId}")
            .SendAsync("TelemetryUpdate", new TelemetryData
            {
                MachineId = machineId.ToString(),
                Timestamp = telemetry.Timestamp,
                Temperature = null,
                Pressure = null,
                Vibration = null,
                RulPrediction = null,
                Metrics = null,
                Message = $"New telemetry data: {telemetry.DataType}"
            });
    }

    /// <inheritdoc />
    public async Task BroadcastPredictionAsync(Guid machineId, PredictionDto prediction)
    {
        _logger.LogDebug("Broadcasting prediction for machine {MachineId}", machineId);

        await _analyticsHubContext.Clients
            .Group($"predictions-{machineId}")
            .SendAsync("PredictionUpdate", new PredictionData
            {
                MachineId = machineId.ToString(),
                Timestamp = prediction.CreatedAt,
                RulRemaining = prediction.RemainingUsefulLifeDays,
                FailureProbability = prediction.FailureProbability,
                ConfidenceLevel = null, // Not in PredictionDto
                PredictionType = prediction.HealthStatus.ToString(),
                FeatureImportance = prediction.FeatureContributions
            });
    }

    /// <inheritdoc />
    public async Task BroadcastAlertAsync(Guid machineId, AlertDto alert)
    {
        _logger.LogDebug("Broadcasting alert for machine {MachineId}: {Message}", machineId, alert.Message);

        // Use the hubs.AlertData type for SignalR communication
        await _analyticsHubContext.Clients
            .Group("alerts")
            .SendAsync("NewAlert", new AlertData
            {
                AlertId = alert.Id.ToString(),
                MachineId = machineId.ToString(),
                Timestamp = alert.CreatedAt,
                Severity = alert.Severity,
                Category = alert.Category ?? "General",
                Message = alert.Message,
                RecommendedAction = alert.RecommendedAction,
                Acknowledged = alert.IsAcknowledged
            });
    }
}

/// <summary>
/// System health data for broadcasting.
/// </summary>
public class SystemHealthData
{
    public string Status { get; set; } = "Healthy";
    public double CpuPercent { get; set; }
    public double MemoryPercent { get; set; }
    public int ActiveConnections { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
