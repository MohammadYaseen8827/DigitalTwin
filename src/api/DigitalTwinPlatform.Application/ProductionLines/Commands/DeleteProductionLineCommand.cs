using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Domain.Entities;
using MediatR;

namespace DigitalTwinPlatform.Application.ProductionLines.Commands;

public sealed record DeleteProductionLineCommand(Guid Id) : IRequest;

internal sealed class DeleteProductionLineCommandHandler(IRepository<ProductionLine> repository)
    : IRequestHandler<DeleteProductionLineCommand>
{
    public async Task Handle(DeleteProductionLineCommand request, CancellationToken cancellationToken)
    {
        var productionLine = await repository.GetAsync(request.Id, cancellationToken);
        if (productionLine is null)
        {
            return;
        }

        await repository.DeleteAsync(productionLine, cancellationToken);
    }
}
