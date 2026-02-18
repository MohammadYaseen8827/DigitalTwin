using DigitalTwinPlatform.Application.ExternalSystems.Dtos;
using DigitalTwinPlatform.Application.ExternalSystems.Services;
using DigitalTwinPlatform.Domain.Common;

namespace DigitalTwinPlatform.Application.ExternalSystems.Services;

public class MockExternalSystemService : IExternalSystemService
{
    private readonly List<ExternalSystemDto> _externalSystems = new();
    private readonly List<SystemIntegrationDto> _systemIntegrations = new();
    private readonly List<DataSynchronizationDto> _dataSynchronizations = new();

    public MockExternalSystemService()
    {
        SeedSampleData();
    }

    private void SeedSampleData()
    {
        // Sample external systems
        var erpSystem = new ExternalSystemDto(
            Guid.NewGuid(),
            "SAP ERP",
            "ERP",
            "https://sap-erp.company.com/api",
            "sap-api-key-123",
            "erp_user",
            ExternalSystemStatus.Connected,
            DateTime.UtcNow.AddHours(-2),
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddHours(-2)
        );

        var cmmsSystem = new ExternalSystemDto(
            Guid.NewGuid(),
            "Maximo CMMS",
            "CMMS",
            "https://maximo.company.com/api",
            "cmms-token-456",
            "cmms_admin",
            ExternalSystemStatus.Connected,
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddDays(-15),
            DateTime.UtcNow.AddHours(-1)
        );

        var scadaSystem = new ExternalSystemDto(
            Guid.NewGuid(),
            "Wonderware SCADA",
            "SCADA",
            "https://scada.company.com/api",
            null,
            "scada_reader",
            ExternalSystemStatus.Disconnected,
            DateTime.MinValue,
            DateTime.UtcNow.AddDays(-5),
            DateTime.UtcNow.AddDays(-2)
        );

        _externalSystems.AddRange([erpSystem, cmmsSystem, scadaSystem]);

        // Sample integrations
        var machineIntegration = new SystemIntegrationDto(
            Guid.NewGuid(),
            erpSystem.Id,
            Guid.NewGuid(),
            EntityType.Machine,
            IntegrationType.ReadWrite,
            true,
            30,
            DateTime.UtcNow.AddDays(-25),
            DateTime.UtcNow
        );

        var maintenanceIntegration = new SystemIntegrationDto(
            Guid.NewGuid(),
            cmmsSystem.Id,
            Guid.NewGuid(),
            EntityType.MaintenanceRecord,
            IntegrationType.ReadOnly,
            true,
            60,
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow
        );

        _systemIntegrations.AddRange([machineIntegration, maintenanceIntegration]);

        // Sample synchronizations
        var recentSync = new DataSynchronizationDto(
            Guid.NewGuid(),
            erpSystem.Id,
            machineIntegration.EntityId,
            EntityType.Machine,
            SyncDirection.Inbound,
            SyncStatus.Completed,
            "{\"machines\": 15, \"updated\": 3}",
            null,
            DateTime.UtcNow.AddMinutes(-30),
            DateTime.UtcNow.AddMinutes(-28),
            DateTime.UtcNow.AddMinutes(-30),
            DateTime.UtcNow.AddMinutes(-28)
        );

        var failedSync = new DataSynchronizationDto(
            Guid.NewGuid(),
            cmmsSystem.Id,
            maintenanceIntegration.EntityId,
            EntityType.MaintenanceRecord,
            SyncDirection.Outbound,
            SyncStatus.Failed,
            null,
            "Connection timeout",
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddMinutes(-55),
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddMinutes(-55)
        );

        var pendingSync = new DataSynchronizationDto(
            Guid.NewGuid(),
            erpSystem.Id,
            Guid.NewGuid(),
            EntityType.ProductionLine,
            SyncDirection.Bidirectional,
            SyncStatus.Pending,
            null,
            null,
            DateTime.UtcNow,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _dataSynchronizations.AddRange([recentSync, failedSync, pendingSync]);
    }

    // External System Management
    public async Task<Result<ExternalSystemDto>> CreateExternalSystemAsync(ExternalSystemCreateDto systemDto, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var newSystem = new ExternalSystemDto(
            Guid.NewGuid(),
            systemDto.Name,
            systemDto.SystemType,
            systemDto.ConnectionUrl,
            systemDto.ApiKey,
            systemDto.Username,
            ExternalSystemStatus.Disconnected,
            DateTime.MinValue,
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _externalSystems.Add(newSystem);
        return new Result<ExternalSystemDto>.Success(newSystem);
    }

    public async Task<Result<ExternalSystemDto>> UpdateExternalSystemAsync(Guid id, ExternalSystemUpdateDto systemDto, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var system = _externalSystems.FirstOrDefault(s => s.Id == id);
        if (system == null)
            return new Result<ExternalSystemDto>.Failure("External system not found");

        var updatedSystem = system with
        {
            Name = systemDto.Name ?? system.Name,
            SystemType = systemDto.SystemType ?? system.SystemType,
            ConnectionUrl = systemDto.ConnectionUrl ?? system.ConnectionUrl,
            ApiKey = systemDto.ApiKey ?? system.ApiKey,
            Username = systemDto.Username ?? system.Username,
            UpdatedAt = DateTime.UtcNow
        };

        _externalSystems.Remove(system);
        _externalSystems.Add(updatedSystem);

        return new Result<ExternalSystemDto>.Success(updatedSystem);
    }

    public async Task<Result> DeleteExternalSystemAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var system = _externalSystems.FirstOrDefault(s => s.Id == id);
        if (system == null)
            return new Result.Failure("External system not found");

        _externalSystems.Remove(system);
        return new Result.Success();
    }

    public async Task<Result<ExternalSystemDto>> GetExternalSystemByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var system = _externalSystems.FirstOrDefault(s => s.Id == id);
        if (system == null)
            return new Result<ExternalSystemDto>.Failure("External system not found");

        return new Result<ExternalSystemDto>.Success(system);
    }

    public async Task<Result<List<ExternalSystemDto>>> GetAllExternalSystemsAsync(CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        return new Result<List<ExternalSystemDto>>.Success(_externalSystems.ToList());
    }

    public async Task<Result<List<ExternalSystemDto>>> GetConnectedExternalSystemsAsync(CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        var connected = _externalSystems.Where(s => s.Status == ExternalSystemStatus.Connected).ToList();
        return new Result<List<ExternalSystemDto>>.Success(connected);
    }

    public async Task<Result<List<ExternalSystemDto>>> GetExternalSystemsByTypeAsync(string systemType, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        var systems = _externalSystems.Where(s => s.SystemType.Equals(systemType, StringComparison.OrdinalIgnoreCase)).ToList();
        return new Result<List<ExternalSystemDto>>.Success(systems);
    }

    public async Task<Result<ExternalSystemStatus>> GetExternalSystemStatusAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var system = _externalSystems.FirstOrDefault(s => s.Id == id);
        if (system == null)
            return new Result<ExternalSystemStatus>.Failure("External system not found");

        return new Result<ExternalSystemStatus>.Success(system.Status);
    }

    public async Task<Result> ConnectExternalSystemAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var system = _externalSystems.FirstOrDefault(s => s.Id == id);
        if (system == null)
            return new Result.Failure("External system not found");

        var updatedSystem = system with
        {
            Status = ExternalSystemStatus.Connected,
            LastConnected = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _externalSystems.Remove(system);
        _externalSystems.Add(updatedSystem);

        return new Result.Success();
    }

    public async Task<Result> DisconnectExternalSystemAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var system = _externalSystems.FirstOrDefault(s => s.Id == id);
        if (system == null)
            return new Result.Failure("External system not found");

        var updatedSystem = system with
        {
            Status = ExternalSystemStatus.Disconnected,
            UpdatedAt = DateTime.UtcNow
        };

        _externalSystems.Remove(system);
        _externalSystems.Add(updatedSystem);

        return new Result.Success();
    }

    public async Task<Result<bool>> TestExternalSystemConnectionAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(1000, ct); // Simulate connection test
        
        var system = _externalSystems.FirstOrDefault(s => s.Id == id);
        if (system == null)
            return new Result<bool>.Failure("External system not found");

        // Simulate random connection success/failure
        bool isConnected = new Random().NextDouble() > 0.3; // 70% success rate
        
        if (isConnected)
        {
            var updatedSystem = system with
            {
                Status = ExternalSystemStatus.Connected,
                LastConnected = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _externalSystems.Remove(system);
            _externalSystems.Add(updatedSystem);
        }
        else
        {
            var updatedSystem = system with
            {
                Status = ExternalSystemStatus.Error,
                UpdatedAt = DateTime.UtcNow
            };
            _externalSystems.Remove(system);
            _externalSystems.Add(updatedSystem);
        }

        return new Result<bool>.Success(isConnected);
    }

    // System Integration Management
    public async Task<Result<SystemIntegrationDto>> CreateSystemIntegrationAsync(SystemIntegrationCreateDto integrationDto, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var system = _externalSystems.FirstOrDefault(s => s.Id == integrationDto.ExternalSystemId);
        if (system == null)
            return new Result<SystemIntegrationDto>.Failure("External system not found");

        var newIntegration = new SystemIntegrationDto(
            Guid.NewGuid(),
            integrationDto.ExternalSystemId,
            integrationDto.EntityId,
            integrationDto.EntityType,
            integrationDto.IntegrationType,
            true,
            integrationDto.SyncIntervalMinutes,
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _systemIntegrations.Add(newIntegration);
        return new Result<SystemIntegrationDto>.Success(newIntegration);
    }

    public async Task<Result<SystemIntegrationDto>> UpdateSystemIntegrationAsync(Guid id, SystemIntegrationUpdateDto integrationDto, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var integration = _systemIntegrations.FirstOrDefault(i => i.Id == id);
        if (integration == null)
            return new Result<SystemIntegrationDto>.Failure("System integration not found");

        var updatedIntegration = integration with
        {
            IntegrationType = integrationDto.IntegrationType ?? integration.IntegrationType,
            SyncIntervalMinutes = integrationDto.SyncIntervalMinutes ?? integration.SyncIntervalMinutes,
            IsEnabled = integrationDto.IsEnabled ?? integration.IsEnabled,
            UpdatedAt = DateTime.UtcNow
        };

        _systemIntegrations.Remove(integration);
        _systemIntegrations.Add(updatedIntegration);

        return new Result<SystemIntegrationDto>.Success(updatedIntegration);
    }

    public async Task<Result> DeleteSystemIntegrationAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var integration = _systemIntegrations.FirstOrDefault(i => i.Id == id);
        if (integration == null)
            return new Result.Failure("System integration not found");

        _systemIntegrations.Remove(integration);
        return new Result.Success();
    }

    public async Task<Result<SystemIntegrationDto>> GetSystemIntegrationByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var integration = _systemIntegrations.FirstOrDefault(i => i.Id == id);
        if (integration == null)
            return new Result<SystemIntegrationDto>.Failure("System integration not found");

        return new Result<SystemIntegrationDto>.Success(integration);
    }

    public async Task<Result<List<SystemIntegrationDto>>> GetSystemIntegrationsAsync(CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        return new Result<List<SystemIntegrationDto>>.Success(_systemIntegrations.ToList());
    }

    public async Task<Result<List<SystemIntegrationDto>>> GetSystemIntegrationsBySystemAsync(Guid externalSystemId, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        var integrations = _systemIntegrations.Where(i => i.ExternalSystemId == externalSystemId).ToList();
        return new Result<List<SystemIntegrationDto>>.Success(integrations);
    }

    public async Task<Result<List<SystemIntegrationDto>>> GetSystemIntegrationsByEntityAsync(Guid entityId, EntityType entityType, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        var integrations = _systemIntegrations.Where(i => i.EntityId == entityId && i.EntityType == entityType).ToList();
        return new Result<List<SystemIntegrationDto>>.Success(integrations);
    }

    public async Task<Result<List<SystemIntegrationDto>>> GetActiveSystemIntegrationsAsync(CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        var active = _systemIntegrations.Where(i => i.IsEnabled).ToList();
        return new Result<List<SystemIntegrationDto>>.Success(active);
    }

    public async Task<Result> EnableSystemIntegrationAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var integration = _systemIntegrations.FirstOrDefault(i => i.Id == id);
        if (integration == null)
            return new Result.Failure("System integration not found");

        var updatedIntegration = integration with { IsEnabled = true, UpdatedAt = DateTime.UtcNow };
        _systemIntegrations.Remove(integration);
        _systemIntegrations.Add(updatedIntegration);

        return new Result.Success();
    }

    public async Task<Result> DisableSystemIntegrationAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var integration = _systemIntegrations.FirstOrDefault(i => i.Id == id);
        if (integration == null)
            return new Result.Failure("System integration not found");

        var updatedIntegration = integration with { IsEnabled = false, UpdatedAt = DateTime.UtcNow };
        _systemIntegrations.Remove(integration);
        _systemIntegrations.Add(updatedIntegration);

        return new Result.Success();
    }

    // Data Synchronization Management
    public async Task<Result<DataSynchronizationDto>> CreateDataSynchronizationAsync(DataSynchronizationCreateDto syncDto, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var system = _externalSystems.FirstOrDefault(s => s.Id == syncDto.ExternalSystemId);
        if (system == null)
            return new Result<DataSynchronizationDto>.Failure("External system not found");

        var newSync = new DataSynchronizationDto(
            Guid.NewGuid(),
            syncDto.ExternalSystemId,
            syncDto.EntityId,
            syncDto.EntityType,
            syncDto.Direction,
            SyncStatus.Pending,
            syncDto.DataPayload,
            null,
            DateTime.UtcNow,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _dataSynchronizations.Add(newSync);
        return new Result<DataSynchronizationDto>.Success(newSync);
    }

    public async Task<Result<DataSynchronizationDto>> GetDataSynchronizationByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var sync = _dataSynchronizations.FirstOrDefault(s => s.Id == id);
        if (sync == null)
            return new Result<DataSynchronizationDto>.Failure("Data synchronization not found");

        return new Result<DataSynchronizationDto>.Success(sync);
    }

    public async Task<Result<List<DataSynchronizationDto>>> GetDataSynchronizationsAsync(CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        return new Result<List<DataSynchronizationDto>>.Success(_dataSynchronizations.ToList());
    }

    public async Task<Result<List<DataSynchronizationDto>>> GetPendingSynchronizationsAsync(CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        var pending = _dataSynchronizations.Where(s => s.Status == SyncStatus.Pending).ToList();
        return new Result<List<DataSynchronizationDto>>.Success(pending);
    }

    public async Task<Result<List<DataSynchronizationDto>>> GetFailedSynchronizationsAsync(CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        var failed = _dataSynchronizations.Where(s => s.Status == SyncStatus.Failed).ToList();
        return new Result<List<DataSynchronizationDto>>.Success(failed);
    }

    public async Task<Result<List<DataSynchronizationDto>>> GetDataSynchronizationsBySystemAsync(Guid externalSystemId, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        var syncs = _dataSynchronizations.Where(s => s.ExternalSystemId == externalSystemId).ToList();
        return new Result<List<DataSynchronizationDto>>.Success(syncs);
    }

    public async Task<Result<List<DataSynchronizationDto>>> GetDataSynchronizationsByEntityAsync(Guid entityId, EntityType entityType, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        var syncs = _dataSynchronizations.Where(s => s.EntityId == entityId && s.EntityType == entityType).ToList();
        return new Result<List<DataSynchronizationDto>>.Success(syncs);
    }

    public async Task<Result<List<DataSynchronizationDto>>> GetRecentSynchronizationsAsync(int limit = 50, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        var recent = _dataSynchronizations.OrderByDescending(s => s.StartedAt).Take(limit).ToList();
        return new Result<List<DataSynchronizationDto>>.Success(recent);
    }

    public async Task<Result<int>> ProcessPendingSynchronizationsAsync(CancellationToken ct = default)
    {
        await Task.Delay(2000, ct); // Simulate processing time
        
        var pendingSyncs = _dataSynchronizations.Where(s => s.Status == SyncStatus.Pending).ToList();
        int processedCount = 0;

        foreach (var sync in pendingSyncs.Take(3)) // Process max 3 at a time
        {
            // Simulate processing
            var processingSync = sync with
            {
                Status = SyncStatus.Processing,
                UpdatedAt = DateTime.UtcNow
            };
            
            _dataSynchronizations.Remove(sync);
            _dataSynchronizations.Add(processingSync);

            await Task.Delay(500, ct); // Simulate work

            // Random success/failure
            bool isSuccess = new Random().NextDouble() > 0.2; // 80% success rate
            
            var finalSync = processingSync with
            {
                Status = isSuccess ? SyncStatus.Completed : SyncStatus.Failed,
                CompletedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DataPayload = isSuccess ? "{\"result\": \"success\", \"records\": 10}" : null,
                ErrorMessage = isSuccess ? null : "Timeout during data transfer"
            };

            _dataSynchronizations.Remove(processingSync);
            _dataSynchronizations.Add(finalSync);
            processedCount++;
        }

        return new Result<int>.Success(processedCount);
    }

    public async Task<Result> CancelDataSynchronizationAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var sync = _dataSynchronizations.FirstOrDefault(s => s.Id == id);
        if (sync == null)
            return new Result.Failure("Data synchronization not found");

        if (sync.Status != SyncStatus.Pending)
            return new Result.Failure("Only pending synchronizations can be cancelled");

        var cancelledSync = sync with
        {
            Status = SyncStatus.Failed,
            ErrorMessage = "Cancelled by user",
            CompletedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dataSynchronizations.Remove(sync);
        _dataSynchronizations.Add(cancelledSync);

        return new Result.Success();
    }

    public async Task<Result<DataSynchronizationDto>> RetryFailedSynchronizationAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var sync = _dataSynchronizations.FirstOrDefault(s => s.Id == id);
        if (sync == null)
            return new Result<DataSynchronizationDto>.Failure("Data synchronization not found");

        if (sync.Status != SyncStatus.Failed)
            return new Result<DataSynchronizationDto>.Failure("Only failed synchronizations can be retried");

        var retriedSync = sync with
        {
            Status = SyncStatus.Pending,
            ErrorMessage = null,
            StartedAt = DateTime.UtcNow,
            CompletedAt = null,
            UpdatedAt = DateTime.UtcNow
        };

        _dataSynchronizations.Remove(sync);
        _dataSynchronizations.Add(retriedSync);

        return new Result<DataSynchronizationDto>.Success(retriedSync);
    }
}