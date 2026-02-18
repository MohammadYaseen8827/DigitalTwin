using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DigitalTwinPlatform.API.Services.Simulation;
using DigitalTwinPlatform.Domain.Entities.Simulation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DigitalTwinPlatform.API.Tests.Services.Simulation
{
    public class SimulationTest
    {
        private readonly Mock<ILogger<SimulationEngine>> _loggerMock;
        private readonly Mock<IDataValidationService> _validationServiceMock;
        private readonly ISimulationEngine _simulationEngine;

        public SimulationTest()
        {
            _loggerMock = new Mock<ILogger<SimulationEngine>>();
            _validationServiceMock = new Mock<IDataValidationService>();
            _simulationEngine = new SimulationEngine(_loggerMock.Object, _validationServiceMock.Object);
        }

        [Fact]
        public async Task RunSimulation_CompletesSuccessfully()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["totalSteps"] = 10,
                ["simulationType"] = "test"
            };

            // Act
            var state = await _simulationEngine.InitializeAsync(parameters);
            
            // Run simulation steps
            for (int i = 1; i <= 10; i++)
            {
                var result = await _simulationEngine.RunStepAsync(state, i);
                Assert.NotNull(result);
                Assert.Equal(state.Id, result.SimulationId);
            }

            // Complete simulation
            var finalResult = await _simulationEngine.CompleteAsync(state);

            // Assert
            Assert.NotNull(finalResult);
            Assert.Equal(SimulationStatus.Completed, state.Status);
            Assert.NotNull(state.EndTime);
        }

        [Fact]
        public async Task PauseAndResume_WorksCorrectly()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["totalSteps"] = 5
            };

            // Act
            var state = await _simulationEngine.InitializeAsync(parameters);
            
            // Run first step
            await _simulationEngine.RunStepAsync(state, 1);
            
            // Pause simulation
            state = await _simulationEngine.PauseAsync(state);
            Assert.Equal(SimulationStatus.Paused, state.Status);
            
            // Resume simulation
            state = await _simulationEngine.ResumeAsync(state);
            Assert.Equal(SimulationStatus.Running, state.Status);
            
            // Complete remaining steps
            for (int i = 2; i <= 5; i++)
            {
                await _simulationEngine.RunStepAsync(state, i);
            }
            
            // Complete simulation
            await _simulationEngine.CompleteAsync(state);
            
            // Assert
            Assert.Equal(SimulationStatus.Completed, state.Status);
        }

        [Fact]
        public async Task CancelSimulation_WorksCorrectly()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["totalSteps"] = 100
            };

            // Act
            var state = await _simulationEngine.InitializeAsync(parameters);
            
            // Run a few steps
            for (int i = 1; i <= 3; i++)
            {
                await _simulationEngine.RunStepAsync(state, i);
            }
            
            // Cancel simulation
            state = await _simulationEngine.CancelAsync(state);
            
            // Assert
            Assert.Equal(SimulationStatus.Failed, state.Status);
            Assert.NotNull(state.EndTime);
        }

        [Fact]
        public async Task RunStepAsync_WithCancellationToken_CancelsCorrectly()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["totalSteps"] = 100
            };
            var cts = new CancellationTokenSource();
            
            // Act
            var state = await _simulationEngine.InitializeAsync(parameters);
            
            // Cancel after a short delay
            cts.CancelAfter(50);
            
            // Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () => 
            {
                for (int i = 1; i <= 100; i++)
                {
                    await _simulationEngine.RunStepAsync(state, i, cts.Token);
                }
            });
        }
    }
}
