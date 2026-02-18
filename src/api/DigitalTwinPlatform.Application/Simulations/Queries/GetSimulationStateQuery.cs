using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Simulations.Models;
using DigitalTwinPlatform.Domain.Entities.Simulation;
using MediatR;

namespace DigitalTwinPlatform.Application.Simulations.Queries;

public sealed record GetSimulationStateQuery(Guid SimulationId, Guid MachineId) : IRequest<SimulationStateDto>;

internal sealed class GetSimulationStateQueryHandler(IRepository<SimulationState> repository)
    : IRequestHandler<GetSimulationStateQuery, SimulationStateDto>
{
    public async Task<SimulationStateDto> Handle(GetSimulationStateQuery request, CancellationToken cancellationToken)
    {
        var state = await repository.GetAsync(request.SimulationId, cancellationToken)
                   ?? throw new KeyNotFoundException("Simulation not found");
        return SimulationMapper.ToStateDto(state);
    }
}
