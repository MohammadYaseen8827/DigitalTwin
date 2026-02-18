namespace DigitalTwinPlatform.Domain.Entities.Simulation
{
    public class SimulationResult
    {
        public Guid Id { get; set; }
        public Guid SimulationId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Data { get; set; } = [];
        public Dictionary<string, double> Metrics { get; set; } = [];
        public List<SimulationEvent> Events { get; set; } = [];
    }

    public class SimulationEvent
    {
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; }= string.Empty;
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = [];
    }
}