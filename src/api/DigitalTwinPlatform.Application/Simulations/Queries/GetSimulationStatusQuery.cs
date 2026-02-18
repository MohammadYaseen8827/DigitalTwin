using System.Linq;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Simulations.Models;
using DigitalTwinPlatform.Domain.Entities.Simulation;
using MediatR;

namespace DigitalTwinPlatform.Application.Simulations.Queries;

public sealed record GetSimulationStatusQuery(Guid MachineId) : IRequest<SimulationStatusDto>;

internal sealed class GetSimulationStatusQueryHandler(IRepository<SimulationState> repository)
    : IRequestHandler<GetSimulationStatusQuery, SimulationStatusDto>
{
    public async Task<SimulationStatusDto> Handle(GetSimulationStatusQuery request, CancellationToken cancellationToken)
    {
        var states = await repository.GetAllAsync(s => s.MachineId == request.MachineId, ct: cancellationToken);
        var latestState = states
            .OrderByDescending(s => s.UpdatedAt)
            .ThenByDescending(s => s.StartTime ?? DateTime.MinValue)
            .FirstOrDefault();

        return SimulationMapper.ToStatusDto(latestState, request.MachineId);
    }
}
