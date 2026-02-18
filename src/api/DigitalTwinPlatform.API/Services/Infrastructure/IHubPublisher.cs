using DigitalTwinPlatform.Application.Telemetry.Models;
using DigitalTwinPlatform.Application.Predictions.Models;
using DigitalTwinPlatform.Application.Alerts.Models;

namespace DigitalTwinPlatform.API.Services.Infrastructure;

/// <summary>
/// Interface for publishing messages to SignalR hubs.
/// Used for real-time communication throughout the application.
/// Extends the Application layer abstraction.
/// </summary>
public interface IHubPublisher : Application.Abstractions.Services.IHubPublisher
{
    // Inherits all methods from Application.Abstractions.Services.IHubPublisher
}
