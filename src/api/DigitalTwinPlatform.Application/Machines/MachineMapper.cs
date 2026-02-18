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
        machine.Properties.RootElement.GetRawText(),
        machine.RemainingUsefulLifeDays,
        machine.FailureProbability,
        machine.HealthStatus?.ToString(),
        machine.CreatedAt,
        machine.UpdatedAt,
        machine.Location,
        machine.InstallationDate,
        machine.LastMaintenanceDate,
        machine.NextMaintenanceDate,
        machine.HealthStatus.HasValue ? MapHealthStatusToScore(machine.HealthStatus.Value) : null,
        ExtractSpecifications(machine.Properties.RootElement.GetRawText()));

    private static string? ExtractSpecifications(string propertiesJson)
    {
        // Extract specifications from the properties JSON
        // This assumes the properties JSON contains a "specifications" field or needs to be transformed
        try
        {
            using var document = JsonDocument.Parse(propertiesJson);
            var root = document.RootElement;
            
            // If there's already a specifications field, return it
            if (root.TryGetProperty("specifications", out var specs))
            {
                return specs.GetRawText();
            }
            
            // Otherwise, return the entire properties as specifications
            return propertiesJson;
        }
        catch
        {
            // If parsing fails, return the original properties
            return propertiesJson;
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
