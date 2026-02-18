using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DigitalTwinPlatform.Domain.Entities.Simulation;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.API.Services.Simulation
{
    public class SimulationService : ISimulationService
    {
        private readonly ISimulationEngine _simulationEngine;
        private readonly ILogger<SimulationService> _logger;
        private readonly Dictionary<Guid, SimulationState> _activeSimulations = [];

        public SimulationService(
            ISimulationEngine simulationEngine,
            ILogger<SimulationService> logger)
        {
            _simulationEngine = simulationEngine ?? throw new ArgumentNullException(nameof(simulationEngine));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<SimulationState> CreateSimulationAsync(Dictionary<string, object> parameters, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating new simulation with parameters: {Parameters}", parameters);
            var state = _simulationEngine.InitializeAsync(parameters).GetAwaiter().GetResult();
            _activeSimulations[state.Id] = state;
            return Task.FromResult(state);
        }

        public Task<SimulationState> GetSimulationStateAsync(Guid simulationId, CancellationToken ct = default)
        {
            if (!_activeSimulations.TryGetValue(simulationId, out var state))
            {
                throw new KeyNotFoundException($"Simulation with ID {simulationId} not found");
            }
            return Task.FromResult(state);
        }

        public async Task<SimulationResult> RunSimulationAsync(Guid simulationId, CancellationToken ct = default)
        {
            var state = await GetSimulationStateAsync(simulationId, ct);
            
            if (state.Status != SimulationStatus.Running)
            {
                throw new InvalidOperationException($"Simulation is in {state.Status} state and cannot be run");
            }

            try
            {
                _logger.LogInformation("Running simulation {SimulationId}", simulationId);
                var result = await _simulationEngine.RunStepAsync(state, state.CurrentStep + 1, ct);
                
                if (state.CurrentStep >= state.TotalSteps)
                {
                    _logger.LogInformation("Simulation {SimulationId} completed all steps", simulationId);
                    await _simulationEngine.CompleteAsync(state, ct);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running simulation {SimulationId}", simulationId);
                state.Status = SimulationStatus.Failed;
                state.EndTime = DateTime.UtcNow;
                throw;
            }
        }

        public async Task<SimulationState> PauseSimulationAsync(Guid simulationId, CancellationToken ct = default)
        {
            var state = await GetSimulationStateAsync(simulationId, ct);
            return await _simulationEngine.PauseAsync(state);
        }

        public async Task<SimulationState> ResumeSimulationAsync(Guid simulationId, CancellationToken ct = default)
        {
            var state = await GetSimulationStateAsync(simulationId, ct);
            return await _simulationEngine.ResumeAsync(state);
        }

        public async Task<SimulationState> CancelSimulationAsync(Guid simulationId, CancellationToken ct = default)
        {
            var state = await GetSimulationStateAsync(simulationId, ct);
            return await _simulationEngine.CancelAsync(state);
        }

        public Task<IEnumerable<SimulationState>> ListSimulationsAsync(CancellationToken ct = default)
        {
            return Task.FromResult(_activeSimulations.Values.AsEnumerable());
        }
    }
}
