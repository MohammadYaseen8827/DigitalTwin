using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Telemetry.Models;
using MediatR;

namespace DigitalTwinPlatform.Application.Telemetry.Queries;

public sealed record GetTelemetrySearchQuery(
    string Query,
    DateTime? Since = null) : IRequest<IReadOnlyCollection<TelemetryDto>>;

internal sealed class GetTelemetrySearchQueryHandler(ITelemetryRepository repository)
    : IRequestHandler<GetTelemetrySearchQuery, IReadOnlyCollection<TelemetryDto>>
{
    public async Task<IReadOnlyCollection<TelemetryDto>> Handle(GetTelemetrySearchQuery request, CancellationToken cancellationToken)
    {
        var telemetryData = await repository.SearchAsync(request.Query, request.Since, cancellationToken);
        return telemetryData.Select(TelemetryMapper.ToDto).ToList();
    }
}