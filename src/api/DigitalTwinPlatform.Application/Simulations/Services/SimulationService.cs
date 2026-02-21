using DigitalTwinPlatform.Application.Simulations.Models;
using DigitalTwinPlatform.Domain.Entities.Simulation;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DigitalTwinPlatform.Application.Simulations.Services;

public class SimulationService : ISimulationService
{
    private readonly DigitalTwinPlatform.Application.Abstractions.UnitOfWork.IUnitOfWork _unitOfWork;
    private readonly ILogger<SimulationService> _logger;

    public SimulationService(
        DigitalTwinPlatform.Application.Abstractions.UnitOfWork.IUnitOfWork unitOfWork,
        ILogger<SimulationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<SimulationStateDto> RunSimulation(CreateSimulationDto simulation)
    {
        _logger.LogInformation("Starting simulation for Machine {MachineId}", simulation.MachineId);
        
        var simulationState = new SimulationState
        {
            Id = Guid.NewGuid(),
            MachineId = simulation.MachineId,
            Status = SimulationStatus.Running,
            CurrentStep = 0,
            TotalSteps = 100, // Default or derived from parameters
            Parameters = simulation.Parameters ?? [],
            Metrics = [],
            CreatedAt = DateTime.UtcNow,
            StartTime = DateTime.UtcNow
        };

        await _unitOfWork.Repository<SimulationState>().AddAsync(simulationState);
        await _unitOfWork.SaveChangesAsync();

        // In a real implementation, we would start a background task or send a message to a queue
        // For now, we just create the state record.

        return MapToDto(simulationState);
    }

    public async Task<SimulationStateDto?> GetSimulationStatus(Guid simulationId)
    {
        var state = await _unitOfWork.Repository<SimulationState>().GetAsync(simulationId);
        return state == null ? null : MapToDto(state);
    }

    public async Task StopSimulation(Guid simulationId)
    {
        _logger.LogInformation("Stopping simulation {SimulationId}", simulationId);
        
        var state = await _unitOfWork.Repository<SimulationState>().GetAsync(simulationId);
        if (state != null)
        {
            state.Status = SimulationStatus.Completed; // Or Stopped if the enum supports it
            state.EndTime = DateTime.UtcNow;
            
            await _unitOfWork.Repository<SimulationState>().UpdateAsync(state);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<SimulationResultDto>> GetSimulationResults(Guid simulationId)
    {
        var results = await _unitOfWork.Repository<SimulationResult>()
            .GetAllAsync(r => r.SimulationId == simulationId);

        return results.Select(r => new SimulationResultDto(
            r.Id,
            r.SimulationId,
            Guid.Empty, // MachineId not in SimulationResult explicitly usually, handled via simulationId relation
            0, // Step not in SimulationResult explicitly usually
            r.Data,
            0.0, // Duration placeholder
            r.Events.Select(e => new SimulationEventDto(
                e.EventType,
                e.Message,
                (e.Timestamp - new DateTime(1970, 1, 1)).TotalSeconds,
                e.Data
            )).ToList(), 
            r.Timestamp
        ));
    }

    private SimulationStateDto MapToDto(SimulationState state)
    {
        return new SimulationStateDto(
            state.Id,
            state.MachineId,
            state.Status,
            state.CurrentStep,
            state.TotalSteps,
            state.Parameters,
            state.Metrics,
            state.CreatedAt,
            state.UpdatedAt,
            state.StartTime,
            state.EndTime
        );
    }
}
