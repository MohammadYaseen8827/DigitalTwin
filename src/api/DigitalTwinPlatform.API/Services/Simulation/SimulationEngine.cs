
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
                // Simulate work
                await Task.Delay(100, ct); // Replace with actual simulation logic
                
                var result = new SimulationResult
                {
                    Id = Guid.NewGuid(),
                    SimulationId = state.Id,
                    Timestamp = DateTime.UtcNow,
                    Data = new Dictionary<string, object>
                    {
                        ["step"] = step,
                        ["value"] = new Random().NextDouble() * 100
                    }
                };

                state.CurrentStep = step;
                state.Metrics["progress"] = (double)step / state.TotalSteps * 100;

                // Statistical Validation
                if (result.Data.TryGetValue("value", out var valObj) && valObj is double val)
                {
                    if (!_generatedValues.ContainsKey(state.Id.ToString()))
                        _generatedValues[state.Id.ToString()] = [];
                    
                    _generatedValues[state.Id.ToString()].Add(val);
                    
                    if (_generatedValues[state.Id.ToString()].Count >= 10)
                    {
                        var validation = validationService.ValidateBatch(_generatedValues[state.Id.ToString()], "generic");
                        state.Metrics["ks_statistic"] = validation.KsStatistic;
                        state.Metrics["autocorrelation"] = validation.Autocorrelation;
                        state.Metrics["data_valid"] = validation.IsValid;
                        
                        // Pass validation data in the result for frontend real-time update
                        result.Data["validation"] = validation;
                    }
                }

                logger.LogDebug("Completed simulation step {Step} for simulation {SimulationId}", step, state.Id);
                return result;
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
