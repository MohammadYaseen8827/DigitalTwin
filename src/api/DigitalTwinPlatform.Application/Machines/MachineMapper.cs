using DigitalTwinPlatform.Application.Machines.Models;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Entities.Enums;
using System.Text.Json;

namespace DigitalTwinPlatform.Application.Machines;

internal static class MachineMapper
{
    public static MachineDto ToDto(Machine machine) => new(
        machine.Id,
        machine.Name.Value,
        machine.Type.Value,
        machine.Status.ToString(),
        machine.Properties.RootElement,
        machine.RemainingUsefulLifeDays,
        machine.FailureProbability,
        machine.HealthStatus?.ToString(),
        machine.CreatedAt,
        machine.UpdatedAt,
        machine.Location,
        machine.InstallationDate,
        machine.LastMaintenanceDate,
        machine.NextMaintenanceDate,
        machine.WarrantyExpiry,
        machine.MaintenanceIntervalDays,
        machine.HealthStatus.HasValue ? MapHealthStatusToScore(machine.HealthStatus.Value) : null,
        ExtractSpecifications(machine.Properties.RootElement),
        machine.SerialNumber,
        machine.Manufacturer,
        machine.Model,
        machine.Criticality);

    private static JsonElement? ExtractSpecifications(JsonElement root)
    {
        try
        {
            // If there's already a specifications field, return it
            if (root.TryGetProperty("specifications", out var specs))
            {
                return specs;
            }
            
            // Otherwise, return null (specifications not found as a sub-property)
            return null;
        }
        catch
        {
            return null;
        }
    }

    private static int MapHealthStatusToScore(HealthClassification status)
    {
        return status switch
        {
            HealthClassification.Healthy => 100,
            HealthClassification.Normal => 85,
            HealthClassification.MinorDegradation => 70,
            HealthClassification.SignificantDegradation => 50,
            HealthClassification.FailureImminent => 25,
            _ => 0
        };
    }
}
