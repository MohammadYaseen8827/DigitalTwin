using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Simulations.Models;
using DigitalTwinPlatform.Domain.Entities.Simulation;
using MediatR;

namespace DigitalTwinPlatform.Application.Simulations.Commands;

public sealed record RunSimulationCommand(Guid SimulationId, Guid MachineId) : IRequest<SimulationResultDto>;

internal sealed class RunSimulationCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<RunSimulationCommand, SimulationResultDto>
{
    public async Task<SimulationResultDto> Handle(RunSimulationCommand request, CancellationToken cancellationToken)
    {
        var stateRepository = unitOfWork.Repository<SimulationState>();
        var resultRepository = unitOfWork.Repository<SimulationResult>();

        var state = await stateRepository.GetAsync(request.SimulationId, cancellationToken)
                   ?? throw new KeyNotFoundException("Simulation not found");

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
}
