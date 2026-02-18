namespace DigitalTwinPlatform.Application.ExternalSystems.Dtos;

public record ExternalSystemDto(
    Guid Id,
    string Name,
    string SystemType,
    string ConnectionUrl,
    string? ApiKey,
    string? Username,
    ExternalSystemStatus Status,
    DateTime LastConnected,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ExternalSystemCreateDto(
    string Name,
    string SystemType,
    string ConnectionUrl,
    string? ApiKey = null,
    string? Username = null,
    string? Password = null);

public record ExternalSystemUpdateDto(
    string? Name = null,
    string? SystemType = null,
    string? ConnectionUrl = null,
    string? ApiKey = null,
    string? Username = null,
    string? Password = null);

public record SystemIntegrationDto(
    Guid Id,
    Guid ExternalSystemId,
    Guid EntityId,
    EntityType EntityType,
    IntegrationType IntegrationType,
    bool IsEnabled,
    int SyncIntervalMinutes,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record SystemIntegrationCreateDto(
    Guid ExternalSystemId,
    Guid EntityId,
    EntityType EntityType,
    IntegrationType IntegrationType = IntegrationType.ReadOnly,
    int SyncIntervalMinutes = 60);

public record SystemIntegrationUpdateDto(
    IntegrationType? IntegrationType = null,
    int? SyncIntervalMinutes = null,
    bool? IsEnabled = null);

public record DataSynchronizationDto(
    Guid Id,
    Guid ExternalSystemId,
    Guid EntityId,
    EntityType EntityType,
    SyncDirection Direction,
    SyncStatus Status,
    string? DataPayload,
    string? ErrorMessage,
    DateTime StartedAt,
    DateTime? CompletedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record DataSynchronizationCreateDto(
    Guid ExternalSystemId,
    Guid EntityId,
    EntityType EntityType,
    SyncDirection Direction = SyncDirection.Inbound,
    string? DataPayload = null);

public enum ExternalSystemStatus
{
    Disconnected = 0,
    Connected = 1,
    Error = 2
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
    ReadOnly = 0,
    ReadWrite = 1,
    WriteOnly = 2
}

public enum SyncDirection
{
    Inbound = 0,
    Outbound = 1,
    Bidirectional = 2
}

public enum SyncStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}