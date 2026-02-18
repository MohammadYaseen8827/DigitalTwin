using DigitalTwinPlatform.Application.ExternalSystems.Dtos;
using DigitalTwinPlatform.Domain.Common;
using MediatR;

namespace DigitalTwinPlatform.Application.ExternalSystems.Queries;

// External System Queries
public record GetExternalSystemByIdQuery(Guid Id) : IRequest<Result<ExternalSystemDto>>;

public record GetAllExternalSystemsQuery : IRequest<Result<List<ExternalSystemDto>>>;

public record GetConnectedExternalSystemsQuery : IRequest<Result<List<ExternalSystemDto>>>;

public record GetExternalSystemByTypeQuery(string SystemType) : IRequest<Result<List<ExternalSystemDto>>>;

public record GetExternalSystemStatusQuery(Guid Id) : IRequest<Result<ExternalSystemStatus>>;

// System Integration Queries
public record GetSystemIntegrationByIdQuery(Guid Id) : IRequest<Result<SystemIntegrationDto>>;

public record GetSystemIntegrationsQuery : IRequest<Result<List<SystemIntegrationDto>>>;

public record GetSystemIntegrationsBySystemQuery(Guid ExternalSystemId) : IRequest<Result<List<SystemIntegrationDto>>>;

public record GetSystemIntegrationsByEntityQuery(Guid EntityId, EntityType EntityType) : IRequest<Result<List<SystemIntegrationDto>>>;

public record GetActiveSystemIntegrationsQuery : IRequest<Result<List<SystemIntegrationDto>>>;

// Data Synchronization Queries
public record GetDataSynchronizationByIdQuery(Guid Id) : IRequest<Result<DataSynchronizationDto>>;

public record GetDataSynchronizationsQuery : IRequest<Result<List<DataSynchronizationDto>>>;

public record GetPendingSynchronizationsQuery : IRequest<Result<List<DataSynchronizationDto>>>;

public record GetFailedSynchronizationsQuery : IRequest<Result<List<DataSynchronizationDto>>>;

public record GetDataSynchronizationsBySystemQuery(Guid ExternalSystemId) : IRequest<Result<List<DataSynchronizationDto>>>;

public record GetDataSynchronizationsByEntityQuery(Guid EntityId, EntityType EntityType) : IRequest<Result<List<DataSynchronizationDto>>>;

public record GetRecentSynchronizationsQuery(int Limit = 50) : IRequest<Result<List<DataSynchronizationDto>>>;