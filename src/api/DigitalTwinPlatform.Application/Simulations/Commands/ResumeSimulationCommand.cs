using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Simulations.Models;
using DigitalTwinPlatform.Domain.Entities.Simulation;
using MediatR;

namespace DigitalTwinPlatform.Application.Simulations.Commands;

public sealed record ResumeSimulationCommand(Guid SimulationId, Guid MachineId) : IRequest<SimulationStateDto>;

internal sealed class ResumeSimulationCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ResumeSimulationCommand, SimulationStateDto>
{
    public async Task<SimulationStateDto> Handle(ResumeSimulationCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<SimulationState>();
        var state = await repository.GetAsync(request.SimulationId, cancellationToken)
                   ?? throw new KeyNotFoundException("Simulation not found");

        state.Status = SimulationStatus.Running;
        state.UpdatedAt = DateTime.UtcNow;
        await repository.UpdateAsync(state, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return SimulationMapper.ToStateDto(state);
    }
}
