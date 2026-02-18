namespace DigitalTwinPlatform.API.Services.Simulation;

public interface ISimulationSchedulerService
{
    Task ScheduleAsync(CancellationToken ct = default);
}
