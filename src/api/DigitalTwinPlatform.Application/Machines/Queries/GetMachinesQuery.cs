using DigitalTwinPlatform.Application.Machines.Models;
using DigitalTwinPlatform.Application.Machines;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using MediatR;

namespace DigitalTwinPlatform.Application.Machines.Queries;

public record GetMachinesQuery : IRequest<IReadOnlyCollection<MachineDto>>;

public class GetMachinesQueryHandler(IMachineRepository repository)
    : IRequestHandler<GetMachinesQuery, IReadOnlyCollection<MachineDto>>
{
    public async Task<IReadOnlyCollection<MachineDto>> Handle(GetMachinesQuery request, CancellationToken cancellationToken)
    {
        var machines = await repository.GetAllAsync(asNoTracking: true, ct: cancellationToken);
        return machines.Select(MachineMapper.ToDto).ToList();
    }
}
