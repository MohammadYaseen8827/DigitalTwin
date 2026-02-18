using System.Linq;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Telemetry.Models;
using MediatR;

namespace DigitalTwinPlatform.Application.Telemetry.Queries;

public sealed record GetRecentTelemetryQuery(
    DateTime? Since = null,
    Guid? MachineId = null,
    int Limit = 100) : IRequest<IReadOnlyCollection<TelemetryDto>>;

internal sealed class GetRecentTelemetryQueryHandler(ITelemetryRepository repository)
    : IRequestHandler<GetRecentTelemetryQuery, IReadOnlyCollection<TelemetryDto>>
{
    public async Task<IReadOnlyCollection<TelemetryDto>> Handle(GetRecentTelemetryQuery request, CancellationToken cancellationToken)
    {
        var telemetryData = await repository.GetRecentAsync(request.Since, request.MachineId, request.Limit, cancellationToken);
        return telemetryData.Select(TelemetryMapper.ToDto).ToList();
    }
}
