using System.ComponentModel.DataAnnotations;
namespace DigitalTwinPlatform.Domain.Entities.Simulation
{
    public class SimulationState
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid MachineId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public SimulationStatus Status { get; set; } = SimulationStatus.Pending;
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        
        [Timestamp]
        public uint  RowVersion { get; set; }
        
        public Dictionary<string, object> Parameters { get; set; } = new();
        public Dictionary<string, object> Metrics { get; set; } = new();
    }

    public enum SimulationStatus
    {
        Pending,
        Running,
        Paused,
        Completed,
        Failed
    }
}