using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Machines.Models;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Enums;
using DigitalTwinPlatform.Domain.ValueObjects;
using MediatR;

namespace DigitalTwinPlatform.Application.Machines.Commands;

public sealed record CreateMachineCommand(MachineCreateDto Dto) : IRequest<MachineDto>;

internal sealed class CreateMachineCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateMachineCommand, MachineDto>
{
    public async Task<MachineDto> Handle(CreateMachineCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        
        // Create value objects with validation
        var nameResult = MachineName.Create(dto.Name);
        if (nameResult is not Domain.Common.Result<MachineName>.Success { Value: var name })
            throw new InvalidOperationException($"Invalid machine name");
        
        var typeResult = MachineType.Create(dto.Type);
        if (typeResult is not Domain.Common.Result<MachineType>.Success { Value: var type })
            throw new InvalidOperationException($"Invalid machine type");
        
        if (!Enum.TryParse<EquipmentStatus>(dto.Status, ignoreCase: true, out var status))
            throw new InvalidOperationException($"Invalid equipment status: {dto.Status}");
        
        // Create machine using factory method
        var machineResult = Machine.Create(name, type);
        if (machineResult is not Domain.Common.Result<Machine>.Success { Value: var machine })
            throw new InvalidOperationException("Failed to create machine");
        
        // Update status if different from default
        if (status != EquipmentStatus.Operational)
        {
            var updateStatusResult = machine.UpdateStatus(status);
            if (updateStatusResult is not Domain.Common.Result.Success)
                throw new InvalidOperationException("Failed to update machine status");
        }
        
        // Update properties if provided
        if (dto.Properties != null)
        {
            var jsonDoc = System.Text.Json.JsonDocument.Parse(dto.Properties.ToString() ?? "{}");
            var updatePropsResult = machine.UpdateProperties(jsonDoc);
            if (updatePropsResult is not Domain.Common.Result.Success)
                throw new InvalidOperationException("Failed to update machine properties");
        }
        
        // Persist using UnitOfWork
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var repository = unitOfWork.Repository<Machine>();
            await repository.AddAsync(machine, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return MachineMapper.ToDto(machine);
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
