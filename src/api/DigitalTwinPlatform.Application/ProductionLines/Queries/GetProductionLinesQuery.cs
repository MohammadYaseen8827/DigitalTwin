using System.Linq;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.ProductionLines.Models;
using DigitalTwinPlatform.Domain.Entities;
using MediatR;

namespace DigitalTwinPlatform.Application.ProductionLines.Queries;

public sealed record GetProductionLinesQuery : IRequest<IReadOnlyCollection<ProductionLineDto>>;

internal sealed class GetProductionLinesQueryHandler(IRepository<ProductionLine> repository)
    : IRequestHandler<GetProductionLinesQuery, IReadOnlyCollection<ProductionLineDto>>
{
    public async Task<IReadOnlyCollection<ProductionLineDto>> Handle(GetProductionLinesQuery request, CancellationToken cancellationToken)
    {
        var productionLines = await repository.GetAllAsync(asNoTracking: true, ct: cancellationToken);
        return productionLines.Select(ProductionLineMapper.ToDto).ToList();
    }
}
