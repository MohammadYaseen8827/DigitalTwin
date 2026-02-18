using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Machines.Models;
using MediatR;

namespace DigitalTwinPlatform.Application.Machines.Queries;

public sealed record GetMachineQuery(Guid Id) : IRequest<MachineDto>;

internal sealed class GetMachineQueryHandler(IMachineRepository repository)
    : IRequestHandler<GetMachineQuery, MachineDto>
{
    public async Task<MachineDto> Handle(GetMachineQuery request, CancellationToken cancellationToken)
    {
        var machine = await repository.GetAsync(request.Id, cancellationToken)
                      ?? throw new KeyNotFoundException("Machine not found");

        return MachineMapper.ToDto(machine);
    }
}
