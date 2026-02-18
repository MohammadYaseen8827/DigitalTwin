using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Simulations.Models;
using DigitalTwinPlatform.Domain.Entities.Simulation;
using MediatR;

namespace DigitalTwinPlatform.Application.Simulations.Commands;

public sealed record PauseSimulationCommand(Guid SimulationId, Guid MachineId) : IRequest<SimulationStateDto>;

internal sealed class PauseSimulationCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<PauseSimulationCommand, SimulationStateDto>
{
    public async Task<SimulationStateDto> Handle(PauseSimulationCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<SimulationState>();
        var state = await repository.GetAsync(request.SimulationId, cancellationToken)
                   ?? throw new KeyNotFoundException("Simulation not found");

        state.Status = SimulationStatus.Paused;
        state.UpdatedAt = DateTime.UtcNow;
        await repository.UpdateAsync(state, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return SimulationMapper.ToStateDto(state);
    }
}
