namespace DigitalTwinPlatform.Application.Machines.Models;

public record MachineDto(
    Guid Id,
    string Name,
    string Type,
    string Status,
    string Properties,
    double? RemainingUsefulLifeDays,
    double? FailureProbability,
    string? HealthStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? Location = null,
    DateTime? InstallationDate = null,
    DateTime? LastMaintenanceDate = null,
    DateTime? NextMaintenanceDate = null,
    int? HealthScore = null,
    string? Specifications = null); // JSON string representing specifications object

public record MachineCreateDto(
    string Name,
    string Type,
    string Status,
    object Properties);

public record MachineUpdateDto(
    string Name,
    string Type,
    string Status,
    object Properties,
    double? RemainingUsefulLifeDays,
    double? FailureProbability,
    string? HealthStatus);
