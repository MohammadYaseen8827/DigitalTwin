using System.Linq;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Simulations.Models;
using DigitalTwinPlatform.Domain.Entities.Simulation;
using MediatR;

namespace DigitalTwinPlatform.Application.Simulations.Queries;

public sealed record ListSimulationsQuery(Guid MachineId) : IRequest<IReadOnlyCollection<SimulationStateDto>>;

internal sealed class ListSimulationsQueryHandler(IRepository<SimulationState> repository)
    : IRequestHandler<ListSimulationsQuery, IReadOnlyCollection<SimulationStateDto>>
{
    public async Task<IReadOnlyCollection<SimulationStateDto>> Handle(ListSimulationsQuery request, CancellationToken cancellationToken)
    {
        var states = await repository.GetAllAsync(s => s.MachineId == request.MachineId, ct: cancellationToken);
        return states.Select(s => SimulationMapper.ToStateDto(s)).ToList();
    }
}
