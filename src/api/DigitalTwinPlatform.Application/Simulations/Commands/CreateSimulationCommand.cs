using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Simulations.Models;
using DigitalTwinPlatform.Domain.Entities.Simulation;
using MediatR;

namespace DigitalTwinPlatform.Application.Simulations.Commands;

public sealed record CreateSimulationCommand(Guid MachineId, Dictionary<string, object> Parameters) : IRequest<SimulationStateDto>;

internal sealed class CreateSimulationCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSimulationCommand, SimulationStateDto>
{
    public async Task<SimulationStateDto> Handle(CreateSimulationCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<SimulationState>();
        var state = new SimulationState
        {
            Id = Guid.NewGuid(),
            Parameters = request.Parameters,
            MachineId = request.MachineId,
            CreatedAt = DateTime.UtcNow,
            StartTime = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = SimulationStatus.Pending,
            CurrentStep = 0,
            TotalSteps = 100
        };

        await repository.AddAsync(state, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return SimulationMapper.ToStateDto(state);
    }
}
