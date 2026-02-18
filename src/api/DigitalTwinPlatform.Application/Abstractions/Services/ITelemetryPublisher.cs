using DigitalTwinPlatform.Application.Telemetry.Models;

namespace DigitalTwinPlatform.Application.Abstractions.Services;

public interface ITelemetryPublisher
{
    Task BroadcastTelemetryAsync(Guid machineId, TelemetryDto telemetry);
}
