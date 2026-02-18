namespace DigitalTwinPlatform.Domain.Models;

public class MachineConfiguration
{
    public string MachineType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DegradationModelConfiguration DegradationModel { get; set; } = null!;
    public List<SensorMappingConfiguration> SensorMappings { get; set; } = new();
    public FailureThresholdConfiguration FailureThresholds { get; set; } = null!;
    public Dictionary<string, object> OperationalParameters { get; set; } = new();
}

public class DegradationModelConfiguration
{
    public string ModelType { get; set; } = string.Empty; // "Wiener", "Exponential", "Markov", "PhysicsInformed"
    public Dictionary<string, double> Parameters { get; set; } = new();
    public Dictionary<string, object> ModelSpecificSettings { get; set; } = new();
}

public class SensorMappingConfiguration
{
    public string SensorType { get; set; } = string.Empty; // "temperature", "vibration", "pressure"
    public string TransferFunction { get; set; } = string.Empty; // Mathematical expression mapping degradation to sensor
    public Dictionary<string, double> TransferFunctionParameters { get; set; } = new();
    public double NoiseLevel { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public class FailureThresholdConfiguration
{
    public double DegradationThreshold { get; set; }
    public Dictionary<string, double> SensorThresholds { get; set; } = new();
    public TimeSpan? MaxOperationalTime { get; set; }
}
