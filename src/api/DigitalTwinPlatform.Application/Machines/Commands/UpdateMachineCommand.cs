using System.Text.Json;
using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Machines.Models;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Entities.Enums;
using DigitalTwinPlatform.Domain.Enums;
using MediatR;

namespace DigitalTwinPlatform.Application.Machines.Commands;

public sealed record UpdateMachineCommand(Guid Id, MachineUpdateDto Dto) : IRequest<MachineDto>;

internal sealed class UpdateMachineCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateMachineCommand, MachineDto>
{
    public async Task<MachineDto> Handle(UpdateMachineCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var repository = unitOfWork.Repository<Machine>();
            var machine = await repository.GetAsync(request.Id, cancellationToken)
                          ?? throw new KeyNotFoundException("Machine not found");

            var dto = request.Dto;
            
            // Update status using domain method
            if (!Enum.TryParse<EquipmentStatus>(dto.Status, ignoreCase: true, out var status))
                throw new InvalidOperationException($"Invalid equipment status: {dto.Status}");
            
            var statusResult = machine.UpdateStatus(status);
            statusResult.Match(
                () => (object?)null,
                error => throw new InvalidOperationException($"Failed to update status: {error}"));
            
            // Update health metrics using domain method
            if (!Enum.TryParse<HealthClassification>(dto.HealthStatus ?? "", ignoreCase: true, out var healthStatus))
                healthStatus = default;
            
            var healthResult = machine.UpdateHealthMetrics(
                dto.RemainingUsefulLifeDays,
                dto.FailureProbability,
                string.IsNullOrEmpty(dto.HealthStatus) ? null : healthStatus);
            
            healthResult.Match(
                () => (object?)null,
                error => throw new InvalidOperationException($"Failed to update health metrics: {error}"));
            
            // Update properties using domain method
            if (dto.Properties != null)
            {
                var jsonDoc = JsonDocument.Parse(dto.Properties.ToString() ?? "{}");
                var propsResult = machine.UpdateProperties(jsonDoc);
                propsResult.Match(
                    () => (object?)null,
                    error => throw new InvalidOperationException($"Failed to update properties: {error}"));
            }

            await repository.UpdateAsync(machine, cancellationToken);
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
