using DigitalTwinPlatform.Domain.Models;

namespace DigitalTwinPlatform.Application.Services;

public interface IMachineConfigurationService
{
    Task<MachineConfiguration> LoadConfigurationAsync(string machineType, CancellationToken ct = default);
    Task<List<MachineConfiguration>> LoadAllConfigurationsAsync(CancellationToken ct = default);
    Task<bool> ValidateConfigurationAsync(string jsonContent, CancellationToken ct = default);
    Task SaveConfigurationAsync(string machineType, MachineConfiguration config, CancellationToken ct = default);
    Task<bool> ConfigurationExistsAsync(string machineType, CancellationToken ct = default);
}
