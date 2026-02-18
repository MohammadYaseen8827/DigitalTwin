using DigitalTwinPlatform.Domain.Entities.Enums;
using DigitalTwinPlatform.Domain.Enums;

namespace DigitalTwinPlatform.Application.Abstractions.Services;

public interface ITwinEngineService
{
    Task UpdateTwinPredictionAsync(Guid machineId, double? rul, double? failureProbability, HealthClassification? health);
    Task<dynamic> GetMachineWithStateAsync(Guid machineId);
}
