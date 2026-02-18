using DigitalTwinPlatform.Application.Telemetry.Models;
using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.Application.Telemetry;

internal static class TelemetryMapper
{
    public static TelemetryDto ToDto(TelemetryData telemetryData) => new(
        telemetryData.Id,
        telemetryData.MachineId,
        telemetryData.DataType,
        telemetryData.Data,
        telemetryData.Timestamp);
}
