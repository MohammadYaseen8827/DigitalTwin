using System.Linq;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Telemetry.Models;
using MediatR;

namespace DigitalTwinPlatform.Application.Telemetry.Queries;

public sealed record GetTelemetryForMachineQuery(
    Guid MachineId,
    DateTime? Since = null,
    int Take = 500) : IRequest<IReadOnlyCollection<TelemetryDto>>;

internal sealed class GetTelemetryForMachineQueryHandler(ITelemetryRepository repository)
    : IRequestHandler<GetTelemetryForMachineQuery, IReadOnlyCollection<TelemetryDto>>
{
    public async Task<IReadOnlyCollection<TelemetryDto>> Handle(GetTelemetryForMachineQuery request, CancellationToken cancellationToken)
    {
        var telemetryData = await repository.GetForMachineAsync(request.MachineId, request.Since, request.Take, cancellationToken);
        return telemetryData.Select(TelemetryMapper.ToDto).ToList();
    }
}
