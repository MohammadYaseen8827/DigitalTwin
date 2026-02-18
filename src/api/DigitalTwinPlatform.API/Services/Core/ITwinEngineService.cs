using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Entities.Enums;
using DigitalTwinPlatform.Domain.Enums;

namespace DigitalTwinPlatform.API.Services.Core;

public interface ITwinEngineService
{
    Task UpdateTwinPredictionAsync(Guid machineId, double? rul, double? failureProbability, HealthClassification? health);
    Task<Machine> GetMachineWithStateAsync(Guid machineId);
}
