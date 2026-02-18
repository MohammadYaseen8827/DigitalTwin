using System.Linq;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Simulations.Models;
using DigitalTwinPlatform.Domain.Entities.Simulation;
using MediatR;

namespace DigitalTwinPlatform.Application.Simulations.Queries;

public sealed record GetAllSimulationStatusesQuery() : IRequest<IReadOnlyDictionary<Guid, SimulationStatusDto>>;

internal sealed class GetAllSimulationStatusesQueryHandler(IRepository<SimulationState> repository)
    : IRequestHandler<GetAllSimulationStatusesQuery, IReadOnlyDictionary<Guid, SimulationStatusDto>>
{
    public async Task<IReadOnlyDictionary<Guid, SimulationStatusDto>> Handle(GetAllSimulationStatusesQuery request, CancellationToken cancellationToken)
    {
        var states = await repository.GetAllAsync(ct: cancellationToken, asNoTracking: true);

        var latestByMachine = states
            .GroupBy(s => s.MachineId)
            .Select(group => new
            {
                MachineId = group.Key,
                State = group
                    .OrderByDescending(s => s.UpdatedAt)
                    .ThenByDescending(s => s.StartTime ?? DateTime.MinValue)
                    .FirstOrDefault()
            })
            .ToDictionary(
                x => x.MachineId,
                x => SimulationMapper.ToStatusDto(x.State, x.MachineId));

        return latestByMachine;
    }
}
