using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Simulations.Models;
using DigitalTwinPlatform.Domain.Entities.Simulation;
using MediatR;

namespace DigitalTwinPlatform.Application.Simulations.Commands;

public sealed record CancelSimulationCommand(Guid SimulationId, Guid MachineId) : IRequest<SimulationStateDto>;

internal sealed class CancelSimulationCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CancelSimulationCommand, SimulationStateDto>
{
    public async Task<SimulationStateDto> Handle(CancelSimulationCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<SimulationState>();
        var state = await repository.GetAsync(request.SimulationId, cancellationToken)
                   ?? throw new KeyNotFoundException("Simulation not found");

        state.Status = SimulationStatus.Failed;
        state.UpdatedAt = DateTime.UtcNow;
        state.EndTime ??= DateTime.UtcNow;
        await repository.UpdateAsync(state, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return SimulationMapper.ToStateDto(state);
    }
}
