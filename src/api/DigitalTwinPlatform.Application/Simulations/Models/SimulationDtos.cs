using DigitalTwinPlatform.Domain.Entities.Simulation;

namespace DigitalTwinPlatform.Application.Simulations.Models;

public record SimulationStateDto(
    Guid Id,
    Guid MachineId,
    SimulationStatus Status,
    int CurrentStep,
    int TotalSteps,
    Dictionary<string, object> Parameters,
    Dictionary<string, object> Metrics,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt);

public record SimulationResultDto(
    Guid Id,
    Guid SimulationId,
    Guid MachineId,
    int Step,
    Dictionary<string, object> Data,
    double Duration,
    List<SimulationEventDto> Events,
    DateTime Timestamp);

public record SimulationEventDto(
    string EventType,
    string Description,
    double Timestamp,
    Dictionary<string, object>? Data);

public record CreateSimulationDto(
    Guid MachineId,
    Dictionary<string, object> Parameters);

public record SimulationStatusDto(
    bool IsRunning,
    bool IsScheduled,
    Guid MachineId,
    string Status,
    DateTime? NextRun,
    DateTime? LastRun,
    string? Error);
