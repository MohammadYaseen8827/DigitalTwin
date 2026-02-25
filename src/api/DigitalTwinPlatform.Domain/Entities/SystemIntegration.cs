using DigitalTwinPlatform.Domain.Common;

namespace DigitalTwinPlatform.Domain.Entities;

public class SystemIntegration : BaseEntity<Guid>
{
    private IntegrationType _integrationType = IntegrationType.ReadOnly;
    private bool _isEnabled = true;
    private int _syncIntervalMinutes = 60;

    // Private constructor for EF Core
    private SystemIntegration() { }

    // Factory method with validation
    public static Result<SystemIntegration> Create(
        Guid externalSystemId,
        Guid entityId,
        EntityType entityType,
        IntegrationType integrationType = IntegrationType.ReadOnly,
        int syncIntervalMinutes = 60)
    {
        if (syncIntervalMinutes < 1)
            return new Result<SystemIntegration>.Failure("Sync interval must be at least 1 minute");

        if (syncIntervalMinutes > 1440)
            return new Result<SystemIntegration>.Failure("Sync interval cannot exceed 1440 minutes (24 hours)");

        var integration = new SystemIntegration
        {
            Id = Guid.NewGuid(),
            ExternalSystemId = externalSystemId,
            EntityId = entityId,
            EntityType = entityType,
            _integrationType = integrationType,
            _syncIntervalMinutes = syncIntervalMinutes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return new Result<SystemIntegration>.Success(integration);
    }

    // Properties
    public Guid ExternalSystemId { get; private set; }
    public Guid EntityId { get; private set; }
    public EntityType EntityType { get; private set; }
    public IntegrationType IntegrationType => _integrationType;
    public bool IsEnabled => _isEnabled;
    public int SyncIntervalMinutes => _syncIntervalMinutes;

    // Navigation properties
    public ExternalSystem ExternalSystem { get; private set; } = null!;

    // Domain methods
    public Result Enable()
    {
        if (_isEnabled)
            return new Result.Failure("Integration is already enabled");

        _isEnabled = true;
        base.UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result Disable()
    {
        if (!_isEnabled)
            return new Result.Failure("Integration is already disabled");

        _isEnabled = false;
        base.UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdateSyncInterval(int minutes)
    {
        if (minutes < 1)
            return new Result.Failure("Sync interval must be at least 1 minute");
        
        if (minutes > 1440)
            return new Result.Failure("Sync interval cannot exceed 1440 minutes (24 hours)");

        _syncIntervalMinutes = minutes;
        base.UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdateIntegrationType(IntegrationType type)
    {
        _integrationType = type;
        base.UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public bool ShouldSync(DateTime lastSyncTime)
    {
        return _isEnabled && 
               DateTime.UtcNow >= lastSyncTime.AddMinutes(_syncIntervalMinutes);
    }
}

public enum EntityType
{
    Machine = 0,
    MaintenanceRecord = 1,
    ProductionLine = 2,
    TelemetryData = 3
}

public enum IntegrationType
{
    ReadOnly = 0,    // Pull data only
    ReadWrite = 1,   // Bidirectional sync
    WriteOnly = 2    // Push data only
}