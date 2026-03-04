using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Simulations.Models;
using DigitalTwinPlatform.Domain.Entities.Simulation;
using MediatR;

namespace DigitalTwinPlatform.Application.Simulations.Commands;

public sealed record ResetSimulationCommand(Guid SimulationId, Guid MachineId) : IRequest<SimulationStateDto>;

internal sealed class ResetSimulationCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ResetSimulationCommand, SimulationStateDto>
{
    public async Task<SimulationStateDto> Handle(ResetSimulationCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<SimulationState>();
        var state = await repository.GetAsync(request.SimulationId, cancellationToken)
                   ?? throw new KeyNotFoundException("Simulation not found");

        // Reset the simulation to Pending state
        state.Status = SimulationStatus.Pending;
        state.UpdatedAt = DateTime.UtcNow;
        state.EndTime = null; // Clear end time to allow restart
        state.CurrentStep = 0; // Reset progress
        
        await repository.UpdateAsync(state, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return SimulationMapper.ToStateDto(state);
    }
}
