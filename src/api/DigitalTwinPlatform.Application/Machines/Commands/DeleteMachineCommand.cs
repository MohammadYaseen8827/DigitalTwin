using DigitalTwinPlatform.Application.Abstractions.Repositories;
using MediatR;

namespace DigitalTwinPlatform.Application.Machines.Commands;

public sealed record DeleteMachineCommand(Guid Id) : IRequest;

internal sealed class DeleteMachineCommandHandler(IMachineRepository repository)
    : IRequestHandler<DeleteMachineCommand>
{
    public async Task Handle(DeleteMachineCommand request, CancellationToken cancellationToken)
    {
        var machine = await repository.GetAsync(request.Id, cancellationToken);
        if (machine is null)
        {
            return;
        }

        await repository.DeleteAsync(machine, cancellationToken);
    }
}
