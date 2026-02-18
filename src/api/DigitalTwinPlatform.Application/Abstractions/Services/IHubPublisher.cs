using DigitalTwinPlatform.Application.Telemetry.Models;
using DigitalTwinPlatform.Application.Predictions.Models;
using DigitalTwinPlatform.Application.Alerts.Models;

namespace DigitalTwinPlatform.Application.Abstractions.Services;

/// <summary>
/// Interface for publishing real-time updates to connected clients.
/// </summary>
public interface IHubPublisher
{
    /// <summary>
    /// Broadcast telemetry data to subscribers.
    /// </summary>
    Task BroadcastTelemetryAsync(Guid machineId, TelemetryDto telemetry);

    /// <summary>
    /// Broadcast prediction data to subscribers.
    /// </summary>
    Task BroadcastPredictionAsync(Guid machineId, PredictionDto prediction);

    /// <summary>
    /// Broadcast alert to subscribers.
    /// </summary>
    Task BroadcastAlertAsync(Guid machineId, AlertDto alert);
}
