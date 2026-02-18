namespace DigitalTwinPlatform.Application.Abstractions.Services;

public interface IPredictionPublisher
{
    Task BroadcastPredictionAsync(Guid machineId, dynamic prediction);
}
