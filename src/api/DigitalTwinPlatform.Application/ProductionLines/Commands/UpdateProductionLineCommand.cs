using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.ProductionLines.Models;
using DigitalTwinPlatform.Domain.Entities;
using MediatR;

namespace DigitalTwinPlatform.Application.ProductionLines.Commands;

public sealed record UpdateProductionLineCommand(Guid Id, ProductionLineUpdateDto Dto) : IRequest<ProductionLineDto>;

internal sealed class UpdateProductionLineCommandHandler(IRepository<ProductionLine> repository)
    : IRequestHandler<UpdateProductionLineCommand, ProductionLineDto>
{
    public async Task<ProductionLineDto> Handle(UpdateProductionLineCommand request, CancellationToken cancellationToken)
    {
        var productionLine = await repository.GetAsync(request.Id, cancellationToken)
                             ?? throw new KeyNotFoundException("Production line not found");

        var dto = request.Dto;
        productionLine.Name = dto.Name;
        productionLine.Configuration = dto.Configuration;

        await repository.UpdateAsync(productionLine, cancellationToken);
        return ProductionLineMapper.ToDto(productionLine);
    }
}
