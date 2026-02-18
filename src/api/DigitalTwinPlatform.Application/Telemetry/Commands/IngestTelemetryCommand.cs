using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Abstractions.Services;
using DigitalTwinPlatform.Application.Telemetry.Models;
using DigitalTwinPlatform.Domain.Entities;
using MediatR;

namespace DigitalTwinPlatform.Application.Telemetry.Commands;

public sealed record IngestTelemetryCommand(TelemetryIngestDto Dto) : IRequest<TelemetryDto>;

internal sealed class IngestTelemetryCommandHandler(
    IUnitOfWork unitOfWork,
    IHubPublisher? hubPublisher = null)
    : IRequestHandler<IngestTelemetryCommand, TelemetryDto>
{
    public async Task<TelemetryDto> Handle(IngestTelemetryCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var repository = unitOfWork.Repository<TelemetryData>();
        var telemetryData = new TelemetryData
        {
            Id = Guid.NewGuid(),
            MachineId = dto.MachineId,
            DataType = dto.DataType,
            Data = dto.Data,
            Timestamp = dto.Timestamp ?? DateTime.UtcNow
        };

        await repository.AddAsync(telemetryData, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var telemetryDto = TelemetryMapper.ToDto(telemetryData);

        // Broadcast telemetry if publisher is available
        if (hubPublisher != null)
        {
            await hubPublisher.BroadcastTelemetryAsync(dto.MachineId, telemetryDto);
        }

        return telemetryDto;
    }
}
