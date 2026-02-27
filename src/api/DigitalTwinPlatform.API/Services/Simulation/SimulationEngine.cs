
using SimulationResult = DigitalTwinPlatform.Domain.Entities.Simulation.SimulationResult;

namespace DigitalTwinPlatform.API.Services.Simulation
{
    public class SimulationEngine(ILogger<SimulationEngine> logger, IDataValidationService validationService)
        : ISimulationEngine
    {
        private readonly Dictionary<string, List<double>> _generatedValues = [];

        public Task<SimulationState> InitializeAsync(Dictionary<string, object> parameters)
        {
            logger.LogInformation("Initializing simulation with parameters: {Parameters}", parameters);
            
            var state = new SimulationState
            {
                Status = SimulationStatus.Running,
                Parameters = parameters,
                TotalSteps = parameters.TryGetValue("totalSteps", out var steps) ? 
                    Convert.ToInt32(steps) : 100,
                Metrics = new Dictionary<string, object>(),
                CurrentStep = 0
            };

            logger.LogInformation("Simulation {SimulationId} initialized", state.Id);
            return Task.FromResult(state);
        }

        public async Task<SimulationResult> RunStepAsync(SimulationState state, int step, CancellationToken ct = default)
        {
            logger.LogDebug("Running simulation step {Step} for simulation {SimulationId}", step, state.Id);
            
            try
            {
                // ── Weibull-based degradation model ──────────────────────────────
                // Retrieve configurable parameters (with sensible defaults).
                double thermalLoad   = Convert.ToDouble(state.Parameters.GetValueOrDefault("thermalLoad",   0.7));
                double vibrationLoad = Convert.ToDouble(state.Parameters.GetValueOrDefault("vibrationLoad", 0.5));
                double weibullBeta   = Convert.ToDouble(state.Parameters.GetValueOrDefault("weibullBeta",   2.5)); // shape
                double weibullEta    = Convert.ToDouble(state.Parameters.GetValueOrDefault("weibullEta", 1000.0)); // scale (cycles)

                // Composite stress factor (normalised 0-1)
                double stressFactor = Math.Clamp(0.5 * thermalLoad + 0.5 * vibrationLoad, 0.01, 1.0);

                // Weibull cumulative hazard: H(t) = (t / η)^β  — where η is scaled by inverse stress
                double effectiveEta = weibullEta / stressFactor;
                double h = Math.Pow(step / effectiveEta, weibullBeta);

                // Health score: starts at 100, decays to 0 over the expected lifetime
                double healthScore  = Math.Clamp(100.0 * Math.Exp(-h), 0.0, 100.0);

                // Sensor readings derived from heat / vibration physics
                double baseTemp    = 60.0 + thermalLoad   * 40.0 * (1.0 - healthScore / 100.0);
                double baseVib     = 2.0  + vibrationLoad * 8.0  * (1.0 - healthScore / 100.0);
                double basePressure = 5.0 + (1.0 - healthScore / 100.0) * 3.0;

                // Small Gaussian noise (±0.5 %)
                Func<double> noise = () => (new Random().NextDouble() - 0.5) * 0.01;

                var result = new SimulationResult
                {
                    Id           = Guid.NewGuid(),
                    SimulationId = state.Id,
                    Timestamp    = DateTime.UtcNow,
                    Data = new Dictionary<string, object>
                    {
                        ["step"]        = step,
                        ["healthScore"] = Math.Round(healthScore, 2),
                        ["temperature"] = Math.Round(baseTemp    * (1 + noise()), 2),
                        ["vibration"]   = Math.Round(baseVib     * (1 + noise()), 3),
                        ["pressure"]    = Math.Round(basePressure* (1 + noise()), 2),
                        ["stressFactor"]= Math.Round(stressFactor, 4),
                        ["weibullH"]    = Math.Round(h, 6)
                    }
                };

                state.CurrentStep        = step;
                state.Metrics["progress"]    = Math.Round((double)step / state.TotalSteps * 100, 1);
                state.Metrics["healthScore"] = Math.Round(healthScore, 2);

                // Statistical validation every 10 steps
                if (_generatedValues.TryGetValue(state.Id.ToString(), out var vals) == false)
                {
                    vals = [];
                    _generatedValues[state.Id.ToString()] = vals;
                }
                vals.Add(healthScore);

                if (vals.Count >= 10)
                {
                    var validation = validationService.ValidateBatch(vals, "generic");
                    state.Metrics["ks_statistic"]   = validation.KsStatistic;
                    state.Metrics["autocorrelation"] = validation.Autocorrelation;
                    state.Metrics["data_valid"]      = validation.IsValid;
                    result.Data["validation"]        = validation;
                }

                logger.LogDebug("Step {Step}: health={Health:F1}%, temp={Temp:F1}°C", step, healthScore, baseTemp);
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error running simulation step {Step} for simulation {SimulationId}", step, state.Id);
                throw;
            }
        }

        public Task<SimulationResult> CompleteAsync(SimulationState state, CancellationToken ct = default)
        {
            logger.LogInformation("Completing simulation {SimulationId}", state.Id);
            
            state.Status = SimulationStatus.Completed;
            state.EndTime = DateTime.UtcNow;

            var result = new SimulationResult
            {
                Id = Guid.NewGuid(),
                SimulationId = state.Id,
                Timestamp = DateTime.UtcNow,
                Data = new Dictionary<string, object>
                {
                    ["message"] = "Simulation completed successfully"
                }
            };

            return Task.FromResult(result);
        }

        public Task<SimulationState> PauseAsync(SimulationState state)
        {
            logger.LogInformation("Pausing simulation {SimulationId}", state.Id);
            state.Status = SimulationStatus.Paused;
            return Task.FromResult(state);
        }

        public Task<SimulationState> ResumeAsync(SimulationState state)
        {
            logger.LogInformation("Resuming simulation {SimulationId}", state.Id);
            state.Status = SimulationStatus.Running;
            return Task.FromResult(state);
        }

        public Task<SimulationState> CancelAsync(SimulationState state)
        {
            logger.LogInformation("Cancelling simulation {SimulationId}", state.Id);
            state.Status = SimulationStatus.Failed;
            state.EndTime = DateTime.UtcNow;
            return Task.FromResult(state);
        }
    }
}
