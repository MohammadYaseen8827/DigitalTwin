using System.Text.Json;

namespace DigitalTwinPlatform.Application.ProductionLines.Models;

public record ProductionLineDto(
    Guid Id,
    string Name,
    JsonDocument Configuration,
    IEnumerable<Guid> MachineIds);

public record ProductionLineCreateDto(
    string Name,
    JsonDocument Configuration);

public record ProductionLineUpdateDto(
    string Name,
    JsonDocument Configuration);
