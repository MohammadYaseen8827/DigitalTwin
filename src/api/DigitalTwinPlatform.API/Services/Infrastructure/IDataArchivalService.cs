namespace DigitalTwinPlatform.API.Services.Infrastructure;

public interface IDataArchivalService
{
    Task ArchiveOldTelemetryAsync(int retentionDays = 30, CancellationToken ct = default);
}
