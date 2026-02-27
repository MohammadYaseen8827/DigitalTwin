using DigitalTwinPlatform.Application.Machines.Models;
using DigitalTwinPlatform.Application.Machines;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Common.Models;
using MediatR;

namespace DigitalTwinPlatform.Application.Machines.Queries;

public record GetMachinesQuery(int Page = 1, int PageSize = 20) : IRequest<PaginatedResponse<MachineDto>>;

public class GetMachinesQueryHandler(IMachineRepository repository)
    : IRequestHandler<GetMachinesQuery, PaginatedResponse<MachineDto>>
{
    public async Task<PaginatedResponse<MachineDto>> Handle(GetMachinesQuery request, CancellationToken cancellationToken)
    {
        var machines = await repository.GetAllAsync(asNoTracking: true, ct: cancellationToken);
        var machineDtos = machines.Select(MachineMapper.ToDto).ToList();
        
        // Manual pagination for now as IRepository doesn't support skip/take directly
        var totalCount = machineDtos.Count;
        var pagedItems = machineDtos
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedResponse<MachineDto>(pagedItems, totalCount, request.Page, request.PageSize);
    }
}
