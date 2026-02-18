using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.ProductionLines.Models;
using DigitalTwinPlatform.Domain.Entities;
using MediatR;

namespace DigitalTwinPlatform.Application.ProductionLines.Queries;

public sealed record GetProductionLineQuery(Guid Id) : IRequest<ProductionLineDto>;

internal sealed class GetProductionLineQueryHandler(IRepository<ProductionLine> repository)
    : IRequestHandler<GetProductionLineQuery, ProductionLineDto>
{
    public async Task<ProductionLineDto> Handle(GetProductionLineQuery request, CancellationToken cancellationToken)
    {
        var productionLine = await repository.GetAsync(request.Id, cancellationToken)
                            ?? throw new KeyNotFoundException("Production line not found");

        return ProductionLineMapper.ToDto(productionLine);
    }
}
