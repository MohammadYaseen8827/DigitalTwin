using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DigitalTwinPlatform.Domain.Entities.Simulation;

namespace DigitalTwinPlatform.API.Services.Simulation
{
    public interface ISimulationService
    {
        Task<SimulationState> CreateSimulationAsync(Dictionary<string, object> parameters, CancellationToken ct = default);
        Task<SimulationState> GetSimulationStateAsync(Guid simulationId, CancellationToken ct = default);
        Task<SimulationResult> RunSimulationAsync(Guid simulationId, CancellationToken ct = default);
        Task<SimulationState> PauseSimulationAsync(Guid simulationId, CancellationToken ct = default);
        Task<SimulationState> ResumeSimulationAsync(Guid simulationId, CancellationToken ct = default);
        Task<SimulationState> CancelSimulationAsync(Guid simulationId, CancellationToken ct = default);
        Task<IEnumerable<SimulationState>> ListSimulationsAsync(CancellationToken ct = default);
    }
}
