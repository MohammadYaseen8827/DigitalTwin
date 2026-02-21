using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigitalTwinPlatform.Application.Telemetry.Models;

public record TelemetryDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("machineId")] Guid MachineId,
    [property: JsonPropertyName("dataType")] string DataType,
    [property: JsonIgnore] JsonDocument Data,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp)
{
    // Expose data as object for proper JSON serialization
    [JsonPropertyName("data")]
    public object DataObject => Data != null ? 
        JsonSerializer.Deserialize<object>(Data.RootElement.GetRawText()) : 
        new { };
}

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
        if (data != null && data.RootElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in data.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Number)
                {
                    metrics[property.Name.ToLower()] = property.Value.GetDouble();
                }
            }
        }
        
        return new TelemetryMetricsDto(
            MachineId: dto.MachineId,
            Temperature: metrics.GetValueOrDefault("temperature", 0),
            Vibration: metrics.GetValueOrDefault("vibration", 0),
            Pressure: metrics.GetValueOrDefault("pressure", 0),
            Humidity: metrics.GetValueOrDefault("humidity", 0),
            Rpm: metrics.GetValueOrDefault("rpm", 0),
            Timestamp: dto.Timestamp,
            LastUpdated: dto.Timestamp);
    }
}
