using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.ProductionLines.Models;
using DigitalTwinPlatform.Domain.Entities;
using MediatR;

namespace DigitalTwinPlatform.Application.ProductionLines.Queries;

public sealed record SearchProductionLinesQuery(string Query) : IRequest<IEnumerable<ProductionLineDto>>;

internal sealed class SearchProductionLinesQueryHandler(IProductionLineRepository repository)
    : IRequestHandler<SearchProductionLinesQuery, IEnumerable<ProductionLineDto>>
{
    public async Task<IEnumerable<ProductionLineDto>> Handle(SearchProductionLinesQuery request, CancellationToken cancellationToken)
    {
        var productionLines = await repository.SearchAsync(request.Query, cancellationToken);
        return productionLines.Select(MapToDto);
    }

    private static ProductionLineDto MapToDto(ProductionLine productionLine) => new(
        productionLine.Id,
        productionLine.Name,
        productionLine.Configuration,
        productionLine.Machines.Select(m => m.Id)
    );
}