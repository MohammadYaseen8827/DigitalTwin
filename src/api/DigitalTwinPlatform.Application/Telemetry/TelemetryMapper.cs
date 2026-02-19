using DigitalTwinPlatform.Application.Telemetry.Models;
using DigitalTwinPlatform.Domain.Entities;
using System.Text.Json;

namespace DigitalTwinPlatform.Application.Telemetry;

internal static class TelemetryMapper
{
    public static TelemetryDto ToDto(TelemetryData telemetryData) => new(
        telemetryData.Id,
        telemetryData.MachineId,
        telemetryData.DataType,
        telemetryData.Data,
        telemetryData.Timestamp);

    public static TelemetryMetricsDto ToMetrics(this TelemetryDto dto)
    {
        // Extract values from the JSON data
        var dataRoot = dto.Data?.RootElement;
        
        double temperature = 0;
        double vibration = 0;
        double? pressure = null;
        double? humidity = null;
        double? rpm = null;
        
        if (dataRoot.HasValue && dataRoot.Value.ValueKind == JsonValueKind.Object)
        {
            if (dataRoot.Value.TryGetProperty("temperature", out var tempElement))
                temperature = tempElement.GetDouble();
            else if (dataRoot.Value.TryGetProperty("value", out var valueElement))
                temperature = valueElement.GetDouble();

            if (dataRoot.Value.TryGetProperty("vibration", out var vibElement))
                vibration = vibElement.GetDouble();

            if (dataRoot.Value.TryGetProperty("pressure", out var pressElement))
                pressure = pressElement.GetDouble();

            if (dataRoot.Value.TryGetProperty("humidity", out var humElement))
                humidity = humElement.GetDouble();

            if (dataRoot.Value.TryGetProperty("rpm", out var rpmElement))
                rpm = rpmElement.GetDouble();
        }

        return new TelemetryMetricsDto(
            dto.MachineId,
            temperature,
            vibration,
            pressure,
            humidity,
            rpm,
            dto.Timestamp,
            dto.Timestamp);
    }
}
