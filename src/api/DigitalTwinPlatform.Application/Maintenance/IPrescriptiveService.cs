using DigitalTwinPlatform.Domain.ValueObjects;

namespace DigitalTwinPlatform.Application.Maintenance;

public interface IPrescriptiveService
{
    Task<IEnumerable<MaintenanceWindow>> RunWhatIfAnalysisAsync(Guid machineId, int daysToSimulate = 30);
    Task<MaintenanceWindow> GetOptimalMaintenanceDateAsync(Guid machineId);
}
