using DigitalTwinPlatform.Application.Simulations.Models;

namespace DigitalTwinPlatform.Application.Simulations.Services;

public interface ISimulationService
{
    Task<SimulationStateDto> RunSimulation(CreateSimulationDto simulation);
    Task<SimulationStateDto?> GetSimulationStatus(Guid simulationId);
    Task StopSimulation(Guid simulationId);
    Task<IEnumerable<SimulationResultDto>> GetSimulationResults(Guid simulationId);
}
