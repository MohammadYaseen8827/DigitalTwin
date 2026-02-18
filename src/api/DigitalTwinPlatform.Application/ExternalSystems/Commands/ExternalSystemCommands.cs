using DigitalTwinPlatform.Application.ExternalSystems.Dtos;
using DigitalTwinPlatform.Domain.Common;
using MediatR;

namespace DigitalTwinPlatform.Application.ExternalSystems.Commands;

// External System Commands
public record CreateExternalSystemCommand(ExternalSystemCreateDto System) : IRequest<Result<ExternalSystemDto>>;

public record UpdateExternalSystemCommand(Guid Id, ExternalSystemUpdateDto System) : IRequest<Result<ExternalSystemDto>>;

public record DeleteExternalSystemCommand(Guid Id) : IRequest<Result>;

public record ConnectExternalSystemCommand(Guid Id) : IRequest<Result>;

public record DisconnectExternalSystemCommand(Guid Id) : IRequest<Result>;

public record TestExternalSystemConnectionCommand(Guid Id) : IRequest<Result<bool>>;

// System Integration Commands
public record CreateSystemIntegrationCommand(SystemIntegrationCreateDto Integration) : IRequest<Result<SystemIntegrationDto>>;

public record UpdateSystemIntegrationCommand(Guid Id, SystemIntegrationUpdateDto Integration) : IRequest<Result<SystemIntegrationDto>>;

public record DeleteSystemIntegrationCommand(Guid Id) : IRequest<Result>;

public record EnableSystemIntegrationCommand(Guid Id) : IRequest<Result>;

public record DisableSystemIntegrationCommand(Guid Id) : IRequest<Result>;

// Data Synchronization Commands
public record CreateDataSynchronizationCommand(DataSynchronizationCreateDto Synchronization) : IRequest<Result<DataSynchronizationDto>>;

public record ProcessPendingSynchronizationsCommand : IRequest<Result<int>>;

public record CancelDataSynchronizationCommand(Guid Id) : IRequest<Result>;

public record RetryFailedSynchronizationCommand(Guid Id) : IRequest<Result<DataSynchronizationDto>>;