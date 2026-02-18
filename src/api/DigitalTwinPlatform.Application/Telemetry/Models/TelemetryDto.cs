using System.Text.Json;

namespace DigitalTwinPlatform.Application.Telemetry.Models;

public record TelemetryDto(
    Guid Id,
    Guid MachineId,
    string DataType,
    JsonDocument Data,
    DateTime Timestamp);

public record TelemetryIngestDto(
    Guid MachineId,
    string DataType,
    JsonDocument Data,
    DateTime? Timestamp);

/// <summary>
/// Flattened telemetry metrics DTO for frontend consumption.
/// Maps the JSON data in TelemetryDto to flat fields.
/// </summary>
public record TelemetryMetricsDto(
    Guid MachineId,
    double Temperature,
    double Vibration,
    double? Pressure,
    double? Humidity,
    double? Rpm,
    DateTime Timestamp,
    DateTime LastUpdated);

/// <summary>
/// Extension methods for converting TelemetryDto to TelemetryMetricsDto
/// </summary>
public static class TelemetryDtoExtensions
{
    public static TelemetryMetricsDto ToMetrics(this TelemetryDto dto)
    {
        var data = dto.Data;
        var metrics = new Dictionary<string, double>();
        
        // Extract metrics from the JSON data
        foreach (var property in data.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Number)
            {
                metrics[property.Name] = property.Value.GetDouble();
            }
        }
        
        return new TelemetryMetricsDto(
            MachineId: dto.MachineId,
            Temperature: metrics.ContainsKey("temperature") ? metrics["temperature"] : 0,
            Vibration: metrics.ContainsKey("vibration") ? metrics["vibration"] : 0,
            Pressure: metrics.ContainsKey("pressure") ? metrics["pressure"] : null,
            Humidity: metrics.ContainsKey("humidity") ? metrics["humidity"] : null,
            Rpm: metrics.ContainsKey("rpm") ? metrics["rpm"] : null,
            Timestamp: dto.Timestamp,
            LastUpdated: dto.Timestamp);
    }
}
