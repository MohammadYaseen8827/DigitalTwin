using System.Text.Json.Serialization;

namespace DigitalTwinPlatform.Application.Machines.Models;

public record MachineDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("properties")] System.Text.Json.JsonElement Properties,
    [property: JsonPropertyName("remainingUsefulLifeDays")] double? RemainingUsefulLifeDays,
    [property: JsonPropertyName("failureProbability")] double? FailureProbability,
    [property: JsonPropertyName("healthStatus")] string? HealthStatus,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt,
    [property: JsonPropertyName("location")] string? Location = null,
    [property: JsonPropertyName("installationDate")] DateTime? InstallationDate = null,
    [property: JsonPropertyName("lastMaintenanceDate")] DateTime? LastMaintenanceDate = null,
    [property: JsonPropertyName("nextMaintenanceDate")] DateTime? NextMaintenanceDate = null,
    [property: JsonPropertyName("warrantyExpiry")] DateTime? WarrantyExpiry = null,
    [property: JsonPropertyName("maintenanceIntervalDays")] int? MaintenanceIntervalDays = null,
    [property: JsonPropertyName("healthScore")] int? HealthScore = null,
    [property: JsonPropertyName("specifications")] System.Text.Json.JsonElement? Specifications = null,
    [property: JsonPropertyName("serialNumber")] string? SerialNumber = null,
    [property: JsonPropertyName("manufacturer")] string? Manufacturer = null,
    [property: JsonPropertyName("model")] string? Model = null,
    [property: JsonPropertyName("criticality")] int? Criticality = null);

public record MachineCreateDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("properties")] object Properties,
    [property: JsonPropertyName("location")] string? Location = null,
    [property: JsonPropertyName("installationDate")] DateTime? InstallationDate = null,
    [property: JsonPropertyName("warrantyExpiry")] DateTime? WarrantyExpiry = null,
    [property: JsonPropertyName("maintenanceIntervalDays")] int? MaintenanceIntervalDays = null,
    [property: JsonPropertyName("serialNumber")] string? SerialNumber = null,
    [property: JsonPropertyName("manufacturer")] string? Manufacturer = null,
    [property: JsonPropertyName("model")] string? Model = null,
    [property: JsonPropertyName("criticality")] int? Criticality = null);

public record MachineUpdateDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("properties")] object Properties,
    [property: JsonPropertyName("remainingUsefulLifeDays")] double? RemainingUsefulLifeDays,
    [property: JsonPropertyName("failureProbability")] double? FailureProbability,
    [property: JsonPropertyName("healthStatus")] string? HealthStatus,
    [property: JsonPropertyName("location")] string? Location = null,
    [property: JsonPropertyName("installationDate")] DateTime? InstallationDate = null,
    [property: JsonPropertyName("lastMaintenanceDate")] DateTime? LastMaintenanceDate = null,
    [property: JsonPropertyName("nextMaintenanceDate")] DateTime? NextMaintenanceDate = null,
    [property: JsonPropertyName("warrantyExpiry")] DateTime? WarrantyExpiry = null,
    [property: JsonPropertyName("maintenanceIntervalDays")] int? MaintenanceIntervalDays = null,
    [property: JsonPropertyName("serialNumber")] string? SerialNumber = null,
    [property: JsonPropertyName("manufacturer")] string? Manufacturer = null,
    [property: JsonPropertyName("model")] string? Model = null,
    [property: JsonPropertyName("criticality")] int? Criticality = null);
