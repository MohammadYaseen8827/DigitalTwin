using System.Linq;
using DigitalTwinPlatform.Application.Simulations.Models;
using DigitalTwinPlatform.Domain.Entities.Simulation;

namespace DigitalTwinPlatform.Application.Simulations;

internal static class SimulationMapper
{
    public static SimulationStateDto ToStateDto(SimulationState state) => new(
        state.Id,
        state.MachineId,
        state.Status,
        state.CurrentStep,
        state.TotalSteps,
        state.Parameters,
        state.Metrics,
        state.CreatedAt,
        state.UpdatedAt,
        state.StartTime,
        state.EndTime);

    public static SimulationResultDto ToResultDto(SimulationResult result, Guid machineId) => new(
        result.Id,
        result.SimulationId,
        machineId,
        result.Data.TryGetValue("step", out var stepObj) ? Convert.ToInt32(stepObj) : 0,
        result.Data,
        result.Metrics.TryGetValue("duration", out double value) ? (double)value : 0,
        [.. result.Events.Select(e => new SimulationEventDto(
            e.EventType,
            e.Message,
            e.Timestamp.Ticks,
            e.Data))],
        result.Timestamp);

    public static SimulationStatusDto ToStatusDto(SimulationState? state, Guid machineId)
    {
        if (state is null)
        {
            return new SimulationStatusDto(
                IsRunning: false,
                IsScheduled: false,
                MachineId: machineId,
                Status: "idle",
                NextRun: null,
                LastRun: null,
                Error: null);
        }

        var status = state.Status switch
        {
            SimulationStatus.Running => "running",
            SimulationStatus.Pending => "scheduled",
            SimulationStatus.Paused => "scheduled",
            SimulationStatus.Completed => "idle",
            SimulationStatus.Failed => "error",
            _ => "idle"
        };

        var isRunning = state.Status == SimulationStatus.Running;
        var isScheduled = state.Status is SimulationStatus.Pending or SimulationStatus.Paused;

        DateTime? nextRun = state.Status == SimulationStatus.Pending ? state.StartTime : null;
        DateTime? lastRun = state.Status switch
        {
            SimulationStatus.Completed => state.EndTime ?? state.UpdatedAt,
            SimulationStatus.Failed => state.EndTime ?? state.UpdatedAt,
            _ => state.UpdatedAt
        };

        string? error = state.Status == SimulationStatus.Failed ? "Simulation encountered an error." : null;

        return new SimulationStatusDto(
            IsRunning: isRunning,
            IsScheduled: isScheduled,
            MachineId: machineId,
            Status: status,
            NextRun: nextRun,
            LastRun: lastRun,
            Error: error);
    }
}
