using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Simulations.Models;
using DigitalTwinPlatform.Domain.Entities.Simulation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DigitalTwinPlatform.Application.Simulations.Commands;

public sealed record RunSimulationCommand(Guid SimulationId, Guid MachineId) : IRequest<SimulationResultDto>;

internal sealed class RunSimulationCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<RunSimulationCommand, SimulationResultDto>
{
    public async Task<SimulationResultDto> Handle(RunSimulationCommand request, CancellationToken cancellationToken)
    {
        const int maxRetries = 3;
        int attempt = 0;

        while (true)
        {
            try
            {
                var stateRepository = unitOfWork.Repository<SimulationState>();
                var resultRepository = unitOfWork.Repository<SimulationResult>();

                var state = await stateRepository.GetAsync(request.SimulationId, cancellationToken)
                           ?? throw new KeyNotFoundException("Simulation not found");

                // Allow running simulations that are in Pending, Paused, Failed, or Completed states
                // Only block if it's currently Running to prevent duplicate execution
                if (state.Status == SimulationStatus.Running)
                {
                    throw new InvalidOperationException($"Simulation {request.SimulationId} is already running since {state.UpdatedAt:yyyy-MM-dd HH:mm:ss}. " +
                        "Use Pause, Cancel, or Reset commands to change its state first.");
                }

                state.Status = SimulationStatus.Running;
                state.UpdatedAt = DateTime.UtcNow;
                await stateRepository.UpdateAsync(state, cancellationToken);

                var result = new SimulationResult
                {
                    Id = Guid.NewGuid(),
                    SimulationId = request.SimulationId,
                    Timestamp = DateTime.UtcNow,
                    Data = state.Parameters,
                    Metrics = new Dictionary<string, double> { { "duration", 0 } },
                    Events = new List<SimulationEvent>()
                };

                await resultRepository.AddAsync(result, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return SimulationMapper.ToResultDto(result, request.MachineId);
            }
            catch (DbUpdateConcurrencyException)
            {
                attempt++;
                if (attempt >= maxRetries)
                {
                    throw new InvalidOperationException($"Failed to update simulation state after {maxRetries} attempts due to concurrent modifications");
                }
                
                // Small delay before retry
                await Task.Delay(50 * attempt, cancellationToken);
            }
        }
    }
}
