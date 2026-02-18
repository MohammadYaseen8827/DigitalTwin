using DigitalTwinPlatform.Application.ExternalSystems.Dtos;
using DigitalTwinPlatform.Domain.Common;

namespace DigitalTwinPlatform.Application.ExternalSystems.Services;

public interface IExternalSystemService
{
    // External System Management
    Task<Result<ExternalSystemDto>> CreateExternalSystemAsync(ExternalSystemCreateDto systemDto, CancellationToken ct = default);
    Task<Result<ExternalSystemDto>> UpdateExternalSystemAsync(Guid id, ExternalSystemUpdateDto systemDto, CancellationToken ct = default);
    Task<Result> DeleteExternalSystemAsync(Guid id, CancellationToken ct = default);
    Task<Result<ExternalSystemDto>> GetExternalSystemByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<List<ExternalSystemDto>>> GetAllExternalSystemsAsync(CancellationToken ct = default);
    Task<Result<List<ExternalSystemDto>>> GetConnectedExternalSystemsAsync(CancellationToken ct = default);
    Task<Result<List<ExternalSystemDto>>> GetExternalSystemsByTypeAsync(string systemType, CancellationToken ct = default);
    Task<Result<ExternalSystemStatus>> GetExternalSystemStatusAsync(Guid id, CancellationToken ct = default);
    Task<Result> ConnectExternalSystemAsync(Guid id, CancellationToken ct = default);
    Task<Result> DisconnectExternalSystemAsync(Guid id, CancellationToken ct = default);
    Task<Result<bool>> TestExternalSystemConnectionAsync(Guid id, CancellationToken ct = default);

    // System Integration Management
    Task<Result<SystemIntegrationDto>> CreateSystemIntegrationAsync(SystemIntegrationCreateDto integrationDto, CancellationToken ct = default);
    Task<Result<SystemIntegrationDto>> UpdateSystemIntegrationAsync(Guid id, SystemIntegrationUpdateDto integrationDto, CancellationToken ct = default);
    Task<Result> DeleteSystemIntegrationAsync(Guid id, CancellationToken ct = default);
    Task<Result<SystemIntegrationDto>> GetSystemIntegrationByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<List<SystemIntegrationDto>>> GetSystemIntegrationsAsync(CancellationToken ct = default);
    Task<Result<List<SystemIntegrationDto>>> GetSystemIntegrationsBySystemAsync(Guid externalSystemId, CancellationToken ct = default);
    Task<Result<List<SystemIntegrationDto>>> GetSystemIntegrationsByEntityAsync(Guid entityId, EntityType entityType, CancellationToken ct = default);
    Task<Result<List<SystemIntegrationDto>>> GetActiveSystemIntegrationsAsync(CancellationToken ct = default);
    Task<Result> EnableSystemIntegrationAsync(Guid id, CancellationToken ct = default);
    Task<Result> DisableSystemIntegrationAsync(Guid id, CancellationToken ct = default);

    // Data Synchronization Management
    Task<Result<DataSynchronizationDto>> CreateDataSynchronizationAsync(DataSynchronizationCreateDto syncDto, CancellationToken ct = default);
    Task<Result<DataSynchronizationDto>> GetDataSynchronizationByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<List<DataSynchronizationDto>>> GetDataSynchronizationsAsync(CancellationToken ct = default);
    Task<Result<List<DataSynchronizationDto>>> GetPendingSynchronizationsAsync(CancellationToken ct = default);
    Task<Result<List<DataSynchronizationDto>>> GetFailedSynchronizationsAsync(CancellationToken ct = default);
    Task<Result<List<DataSynchronizationDto>>> GetDataSynchronizationsBySystemAsync(Guid externalSystemId, CancellationToken ct = default);
    Task<Result<List<DataSynchronizationDto>>> GetDataSynchronizationsByEntityAsync(Guid entityId, EntityType entityType, CancellationToken ct = default);
    Task<Result<List<DataSynchronizationDto>>> GetRecentSynchronizationsAsync(int limit = 50, CancellationToken ct = default);
    Task<Result<int>> ProcessPendingSynchronizationsAsync(CancellationToken ct = default);
    Task<Result> CancelDataSynchronizationAsync(Guid id, CancellationToken ct = default);
    Task<Result<DataSynchronizationDto>> RetryFailedSynchronizationAsync(Guid id, CancellationToken ct = default);
}