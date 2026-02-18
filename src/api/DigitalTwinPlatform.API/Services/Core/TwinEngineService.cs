using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Enums;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Domain.Entities.Enums;
using AppITwinEngineService = DigitalTwinPlatform.Application.Abstractions.Services.ITwinEngineService;

namespace DigitalTwinPlatform.API.Services.Core;

public class TwinEngineService : ITwinEngineService, AppITwinEngineService
{
    private readonly IMachineRepository _machineRepository;

    public TwinEngineService(IMachineRepository machineRepository)
    {
        _machineRepository = machineRepository;
    }

    public async Task UpdateTwinPredictionAsync(Guid machineId, double? rul, double? failureProbability, HealthClassification? health)
    {
        var machine = await _machineRepository.GetAsync(machineId) ?? throw new KeyNotFoundException("Machine not found");
        var result = machine.UpdateHealthMetrics(rul, failureProbability, health);
        if (result is not Domain.Common.Result.Success)
            throw new InvalidOperationException("Failed to update machine health metrics");
        await _machineRepository.UpdateAsync(machine);
    }

    public async Task<Machine> GetMachineWithStateAsync(Guid machineId)
        => await _machineRepository.GetAsync(machineId) ?? throw new KeyNotFoundException("Machine not found");

    async Task<dynamic> AppITwinEngineService.GetMachineWithStateAsync(Guid machineId)
        => await GetMachineWithStateAsync(machineId);
}

