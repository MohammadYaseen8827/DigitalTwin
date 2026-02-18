using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.ProductionLines.Models;
using DigitalTwinPlatform.Domain.Entities;
using MediatR;

namespace DigitalTwinPlatform.Application.ProductionLines.Commands;

public sealed record CreateProductionLineCommand(ProductionLineCreateDto Dto) : IRequest<ProductionLineDto>;

internal sealed class CreateProductionLineCommandHandler(IRepository<ProductionLine> repository)
    : IRequestHandler<CreateProductionLineCommand, ProductionLineDto>
{
    public async Task<ProductionLineDto> Handle(CreateProductionLineCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var productionLine = new ProductionLine
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Configuration = dto.Configuration
        };

        await repository.AddAsync(productionLine, cancellationToken);
        return ProductionLineMapper.ToDto(productionLine);
    }
}
