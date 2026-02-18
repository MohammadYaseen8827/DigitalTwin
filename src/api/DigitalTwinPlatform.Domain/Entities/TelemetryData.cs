using System.Text.Json;

namespace DigitalTwinPlatform.Domain.Entities;

public class TelemetryData
{
    public Guid Id { get; set; }
    public Guid MachineId { get; set; }
    public Machine? Machine { get; set; }
    public string DataType { get; set; } = string.Empty;
    
    // Core sensor readings
    public double? Temperature { get; set; }
    public double? Vibration { get; set; }
    public double? Pressure { get; set; }
    public double? Rpm { get; set; }
    
    // Computed health score
    public double? HealthScore { get; set; }
    
    // Flexible data storage for additional metrics
    public JsonDocument Data { get; set; } = JsonDocument.Parse("{}");
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a new telemetry record from sensor readings
    /// </summary>
    public static TelemetryData Create(
        Guid machineId,
        double? temperature = null,
        double? vibration = null,
        double? pressure = null,
        double? rpm = null,
        double? healthScore = null)
    {
        return new TelemetryData
        {
            Id = Guid.NewGuid(),
            MachineId = machineId,
            Temperature = temperature,
            Vibration = vibration,
            Pressure = pressure,
            Rpm = rpm,
            HealthScore = healthScore,
            Timestamp = DateTime.UtcNow,
            DataType = "sensor_reading"
        };
    }

    /// <summary>
    /// Computes derived health metrics from raw sensor data
    /// </summary>
    public void ComputeHealthMetrics()
    {
        if (HealthScore.HasValue) return;

        double score = 100.0;
        
        // Deduct for high temperature (ideal is 20-60°C)
        if (Temperature.HasValue)
        {
            if (Temperature > 80) score -= 30;
            else if (Temperature > 70) score -= 20;
            else if (Temperature > 60) score -= 10;
        }
        
        // Deduct for high vibration (ideal is 0-5 mm/s)
        if (Vibration.HasValue)
        {
            if (Vibration > 10) score -= 25;
            else if (Vibration > 7) score -= 15;
            else if (Vibration > 5) score -= 8;
        }
        
        // Deduct for abnormal pressure (ideal is 100-150 PSI)
        if (Pressure.HasValue)
        {
            if (Pressure > 200) score -= 20;
            else if (Pressure > 180) score -= 12;
            else if (Pressure > 150) score -= 5;
        }
        
        // Deduct for abnormal RPM (ideal is 1000-3000)
        if (Rpm.HasValue)
        {
            if (Rpm > 3500) score -= 15;
            else if (Rpm > 3200) score -= 10;
            else if (Rpm < 500) score -= 10;
        }

        HealthScore = Math.Max(0, Math.Min(100, score));
    }
}
