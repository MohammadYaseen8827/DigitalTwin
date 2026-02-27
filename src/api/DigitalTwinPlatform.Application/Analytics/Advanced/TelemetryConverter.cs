using System.Text.Json;
using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.Application.Analytics.Advanced;

/// <summary>
/// Helper class to convert TelemetryData entities to flat telemetry objects for analytics processing
/// </summary>
public class TelemetryConverter
{
    public static FlatTelemetry Convert(TelemetryData telemetryData)
    {
        var flat = new FlatTelemetry
        {
            Id = telemetryData.Id,
            MachineId = telemetryData.MachineId,
            Timestamp = telemetryData.Timestamp,
            DataType = telemetryData.DataType
        };

        try
        {
            // Parse the JSON data to extract common telemetry metrics
            if (telemetryData.Data.RootElement.TryGetProperty("value", out var valueElement))
            {
                if (double.TryParse(valueElement.ToString(), out var value))
                {
                    switch (telemetryData.DataType.ToLower())
                    {
                        case "temperature":
                            flat.Temperature = value;
                            break;
                        case "vibration":
                            flat.Vibration = value;
                            break;
                        case "pressure":
                            flat.Pressure = value;
                            break;
                    }
                }
            }

            // Try to extract additional properties
            if (telemetryData.Data.RootElement.TryGetProperty("temperature", out var tempElement))
            {
                flat.Temperature = tempElement.GetDouble();
            }
            
            if (telemetryData.Data.RootElement.TryGetProperty("vibration", out var vibElement))
            {
                flat.Vibration = vibElement.GetDouble();
            }
            
            if (telemetryData.Data.RootElement.TryGetProperty("pressure", out var pressElement))
            {
                flat.Pressure = pressElement.GetDouble();
            }
        }
        catch (JsonException)
        {
            // If JSON parsing fails, use default values
        }

        return flat;
    }

    public static IEnumerable<FlatTelemetry> Convert(IEnumerable<TelemetryData> telemetryData)
    {
        return telemetryData.Select(Convert);
    }
}

public class FlatTelemetry
{
    public Guid Id { get; set; }
    public Guid MachineId { get; set; }
    public DateTime Timestamp { get; set; }
    public string DataType { get; set; } = string.Empty;
    
    // Common telemetry metrics
    public double Temperature { get; set; }
    public double Vibration { get; set; }
    public double? Pressure { get; set; }
    
    // Additional metrics that might be present
    public double? RotationalSpeed { get; set; }
    public double? Current { get; set; }
    public double? Voltage { get; set; }
}
