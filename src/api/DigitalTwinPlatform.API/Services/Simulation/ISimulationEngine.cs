namespace DigitalTwinPlatform.API.Services.Simulation
{
    public interface ISimulationEngine
    {
        Task<SimulationState> InitializeAsync(Dictionary<string, object> parameters);
        Task<SimulationResult> RunStepAsync(SimulationState state, int step, CancellationToken ct = default);
        Task<SimulationResult> CompleteAsync(SimulationState state, CancellationToken ct = default);
        Task<SimulationState> PauseAsync(SimulationState state);
        Task<SimulationState> ResumeAsync(SimulationState state);
        Task<SimulationState> CancelAsync(SimulationState state);
    }
}
