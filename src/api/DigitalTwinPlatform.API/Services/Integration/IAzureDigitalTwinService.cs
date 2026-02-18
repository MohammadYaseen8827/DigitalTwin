namespace DigitalTwinPlatform.API.Services.Integration;

public interface IAzureDigitalTwinService
{
    Task SyncMachineAsync(Guid machineId);
    Task SyncProductionLineAsync(Guid productionLineId);
    Task UpsertTwinAsync(Guid id, string modelId, IDictionary<string, object>? properties = null);
}
