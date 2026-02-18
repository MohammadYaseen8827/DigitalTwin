namespace DigitalTwinPlatform.API.Services.Simulation;

public class SimulationOptions
{
    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 30;
    public int MachinesPerBatch { get; set; } = 5;
    public bool OnlyActiveMachines { get; set; } = true;
    public List<Guid> MachineIds { get; set; } = [];
}
