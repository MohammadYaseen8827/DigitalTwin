using DigitalTwinPlatform.Domain.Common;

namespace DigitalTwinPlatform.Domain.Entities;

public class DataSynchronization : BaseEntity<Guid>
{
    private SyncDirection _direction = SyncDirection.Inbound;
    private SyncStatus _status = SyncStatus.Pending;

    // Private constructor for EF Core
    private DataSynchronization() { }

    // Factory method with validation
    public static Result<DataSynchronization> Create(
        Guid externalSystemId,
        Guid entityId,
        EntityType entityType,
        SyncDirection direction = SyncDirection.Inbound,
        string? dataPayload = null)
    {
        var sync = new DataSynchronization
        {
            Id = Guid.NewGuid(),
            ExternalSystemId = externalSystemId,
            EntityId = entityId,
            EntityType = entityType,
            _direction = direction,
            DataPayload = dataPayload,
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return new Result<DataSynchronization>.Success(sync);
    }

    // Properties
    public Guid ExternalSystemId { get; private set; }
    public Guid EntityId { get; private set; }
    public EntityType EntityType { get; private set; }
    public SyncDirection Direction => _direction;
    public SyncStatus Status => _status;
    public string? DataPayload { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // Navigation properties
    public ExternalSystem ExternalSystem { get; private set; } = null!;

    // Domain methods
    public Result MarkAsCompleted(string? resultData = null)
    {
        if (_status != SyncStatus.Pending && _status != SyncStatus.Processing)
            return new Result.Failure("Cannot complete synchronization that is not pending or processing");

        _status = SyncStatus.Completed;
        DataPayload = resultData;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result MarkAsFailed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            return new Result.Failure("Error message is required");

        _status = SyncStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result MarkAsProcessing()
    {
        if (_status != SyncStatus.Pending)
            return new Result.Failure("Can only process pending synchronizations");

        _status = SyncStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdateDataPayload(string? payload)
    {
        DataPayload = payload;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public bool IsPending() => _status == SyncStatus.Pending;
    public bool IsProcessing() => _status == SyncStatus.Processing;
    public bool IsCompleted() => _status == SyncStatus.Completed;
    public bool IsFailed() => _status == SyncStatus.Failed;

    public TimeSpan? GetDuration()
    {
        if (StartedAt == default || !CompletedAt.HasValue)
            return null;
        
        return CompletedAt.Value - StartedAt;
    }
}

public enum SyncDirection
{
    Inbound = 0,   // External system → Platform
    Outbound = 1,  // Platform → External system
    Bidirectional = 2 // Both directions
}

public enum SyncStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}