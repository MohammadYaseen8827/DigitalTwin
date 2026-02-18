using System.Net;
using System.Net.Http.Json;
using DigitalTwinPlatform.Application.Simulations.Models;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Entities.Enums;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinPlatform.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Sequential")]
public class SimulationIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly IntegrationTestFixture _fixture;

    public SimulationIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task Simulation_Workflow_Complete_Cycle()
    {
        // Arrange
        var machineId = await CreateTestMachine();
        
        var simulationRequest = new
        {
            MachineId = machineId,
            DurationHours = 24,
            SamplingIntervalSeconds = 60,
            DegradationModel = "Exponential",
            InitialHealthState = 1.0,
            EnvironmentalFactors = new
            {
                Temperature = 75.0,
                Humidity = 60.0,
                LoadFactor = 0.8
            }
        };

        // Act & Assert - Start Simulation
        var startResponse = await _client.PostAsJsonAsync("/api/simulation/start", simulationRequest);
        startResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var startResult = await startResponse.Content.ReadFromJsonAsync<SimulationStartResponse>();
        startResult.Should().NotBeNull();
        startResult!.SimulationId.Should().NotBeEmpty();
        startResult.Status.Should().Be("Running");

        var simulationId = startResult.SimulationId;

        // Act & Assert - Monitor Simulation Progress
        await WaitForSimulationProgress(simulationId, 25); // Wait for at least 25% completion

        var statusResponse = await _client.GetAsync($"/api/simulation/{simulationId}/status");
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var statusResult = await statusResponse.Content.ReadFromJsonAsync<SimulationStatus>();
        statusResult.Should().NotBeNull();
        statusResult!.SimulationId.Should().Be(simulationId);
        statusResult.ProgressPercentage.Should().BeGreaterOrEqualTo(25);

        // Act & Assert - Get Simulation Results
        var resultsResponse = await _client.GetAsync($"/api/simulation/{simulationId}/results");
        resultsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var results = await resultsResponse.Content.ReadFromJsonAsync<SimulationResults>();
        results.Should().NotBeNull();
        results!.DataPoints.Should().NotBeEmpty();
        results.MachineId.Should().Be(machineId);

        // Verify data point structure
        var firstDataPoint = results.DataPoints.First();
        firstDataPoint.Timestamp.Should().NotBe(default(DateTime));
        firstDataPoint.HealthIndicator.Should().BeGreaterThan(0);
        firstDataPoint.Temperature.Should().BeGreaterThan(0);

        // Act & Assert - Cancel Simulation (cleanup)
        var cancelResponse = await _client.PostAsync($"/api/simulation/{simulationId}/cancel", null);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Run_To_Failure_Simulation_Generates_Degradation_Trajectory()
    {
        // Arrange
        var machineId = await CreateTestMachine();
        
        var runToFailureRequest = new
        {
            MachineId = machineId,
            SimulationType = "RunToFailure",
            FailureThreshold = 0.1, // 10% health remaining
            MaxDurationHours = 500, // Allow up to 500 hours
            SamplingIntervalSeconds = 300, // Every 5 minutes
            DegradationModel = "WienerProcess",
            InitialHealthState = 1.0,
            ModelParameters = new
            {
                Drift = -0.001,
                Volatility = 0.02,
                EnvironmentalStress = 1.2
            }
        };

        // Act
        var startResponse = await _client.PostAsJsonAsync("/api/simulation/run-to-failure", runToFailureRequest);
        startResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var startResult = await startResponse.Content.ReadFromJsonAsync<SimulationStartResponse>();
        startResult.Should().NotBeNull();
        var simulationId = startResult!.SimulationId;

        // Wait for completion or significant progress
        await WaitForSimulationProgress(simulationId, 50);

        // Assert - Verify degradation trajectory
        var resultsResponse = await _client.GetAsync($"/api/simulation/{simulationId}/results");
        resultsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var results = await resultsResponse.Content.ReadFromJsonAsync<SimulationResults>();
        results.Should().NotBeNull();
        results!.DataPoints.Should().HaveCountGreaterThan(10); // Should have substantial data

        // Verify health degradation trend
        var healthValues = results.DataPoints.Select(dp => dp.HealthIndicator).ToArray();
        var initialHealth = healthValues.First();
        var finalHealth = healthValues.Last();
        
        // Health should degrade over time (final < initial)
        finalHealth.Should().BeLessThan(initialHealth);
        
        // Should reach near failure threshold
        finalHealth.Should().BeLessOrEqualTo(0.15); // Within 5% of threshold

        // Verify monotonic degradation (generally decreasing)
        var degradationTrend = CalculateTrend(healthValues);
        degradationTrend.Should().BeLessThan(0.1); // Mostly decreasing trend
    }

    [Fact]
    public async Task Configuration_Driven_Simulation_Uses_JSON_Configuration()
    {
        // Arrange - Create machine with specific configuration
        var configMachine = new
        {
            Name = "Config-Driven Test Machine",
            Type = "CNC",
            SerialNumber = "CONFIG-" + Guid.NewGuid().ToString("N")[..8],
            Manufacturer = "TestCorp",
            Model = "TC-2000",
            InstallationDate = DateTime.UtcNow.AddYears(-2),
            Location = "Config Test Lab",
            Criticality = 4,
            Status = "Operational",
            Configuration = new
            {
                DegradationModels = new object[]
                {
                    new
                    {
                        Name = "bearing_wear",
                        Type = "Exponential",
                        Parameters = new
                        {
                            Lambda = 0.0005,
                            EnvironmentalFactor = 1.3
                        }
                    },
                    new
                    {
                        Name = "thermal_stress",
                        Type = "Linear",
                        Parameters = new
                        {
                            Rate = 0.0001,
                            Threshold = 85.0
                        }
                    }
                },
                Sensors = new[]
                {
                    new
                    {
                        Name = "TemperatureSensor",
                        Type = "Thermocouple",
                        Mapping = "temp_actual",
                        NoiseLevel = 0.5
                    },
                    new
                    {
                        Name = "VibrationSensor",
                        Type = "Accelerometer",
                        Mapping = "vibration_actual",
                        NoiseLevel = 0.1
                    }
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/machines", configMachine);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var createdMachine = await createResponse.Content.ReadFromJsonAsync<MachineDto>();
        var machineId = createdMachine!.Id;

        // Act - Start configuration-driven simulation
        var configSimRequest = new
        {
            MachineId = machineId,
            DurationHours = 48,
            UseConfiguration = true, // Use machine's JSON configuration
            OverrideParameters = new
            {
                EnvironmentalFactors = new
                {
                    Temperature = 78.0,
                    LoadFactor = 0.9
                }
            }
        };

        var startResponse = await _client.PostAsJsonAsync("/api/simulation/config-driven", configSimRequest);
        startResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var startResult = await startResponse.Content.ReadFromJsonAsync<SimulationStartResponse>();
        startResult.Should().NotBeNull();
        var simulationId = startResult!.SimulationId;

        // Wait for completion
        await WaitForSimulationCompletion(simulationId);

        // Assert - Verify simulation used configuration
        var resultsResponse = await _client.GetAsync($"/api/simulation/{simulationId}/results");
        resultsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var results = await resultsResponse.Content.ReadFromJsonAsync<SimulationResults>();
        results.Should().NotBeNull();
        
        // Should have rich sensor data based on configuration
        results!.DataPoints.Should().NotBeEmpty();
        results.DataPoints.First().Should().NotBeNull();
        
        // Verify multiple sensor readings exist
        results.DataPoints.Should().OnlyContain(dp => 
            dp.Temperature > 0 && 
            dp.Vibration >= 0 &&
            dp.Pressure.HasValue);
    }

    [Fact]
    public async Task Simulation_Data_Integrates_With_Telemetry_System()
    {
        // Arrange
        var machineId = await CreateTestMachine();
        
        // Get initial telemetry count
        var initialTelemetryResponse = await _client.GetAsync($"/api/telemetry/machine/{machineId}?take=1000");
        var initialTelemetry = await initialTelemetryResponse.Content.ReadFromJsonAsync<List<object>>();
        var initialCount = initialTelemetry?.Count ?? 0;

        // Act - Run simulation that generates telemetry
        var telemetrySimRequest = new
        {
            MachineId = machineId,
            DurationHours = 6, // 6 hours
            SamplingIntervalSeconds = 600, // Every 10 minutes
            GenerateTelemetry = true, // Flag to save to telemetry system
            DegradationModel = "Linear",
            InitialHealthState = 0.95
        };

        var startResponse = await _client.PostAsJsonAsync("/api/simulation/generate-telemetry", telemetrySimRequest);
        startResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var startResult = await startResponse.Content.ReadFromJsonAsync<SimulationStartResponse>();
        var simulationId = startResult!.SimulationId;

        // Wait for completion
        await WaitForSimulationCompletion(simulationId);

        // Assert - Verify telemetry was generated and stored
        var finalTelemetryResponse = await _client.GetAsync($"/api/telemetry/machine/{machineId}?take=1000");
        finalTelemetryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var finalTelemetry = await finalTelemetryResponse.Content.ReadFromJsonAsync<List<TelemetryDto>>();
        finalTelemetry.Should().NotBeNull();
        
        // Should have more telemetry points now
        finalTelemetry!.Count.Should().BeGreaterThan(initialCount);
        
        // Verify generated telemetry has realistic values
        finalTelemetry.Skip(initialCount).Should().OnlyContain(t => 
            t.Temperature >= 60 && t.Temperature <= 100 &&
            t.Vibration >= 0 && t.Vibration <= 2.0 &&
            t.Timestamp > DateTime.UtcNow.AddHours(-7)); // Within last 7 hours
    }

    [Fact]
    public async Task Batch_Simulation_Processing_Handles_Multiple_Machines()
    {
        // Arrange - Create multiple machines
        var machineIds = new List<Guid>();
        for (int i = 0; i < 3; i++)
        {
            var machineId = await CreateTestMachine();
            machineIds.Add(machineId);
        }

        var batchRequest = new
        {
            MachineIds = machineIds,
            SimulationParameters = new
            {
                DurationHours = 24,
                SamplingIntervalSeconds = 1200, // Every 20 minutes
                DegradationModel = "Exponential",
                InitialHealthState = 1.0
            },
            BatchSize = 2, // Process 2 at a time
            Priority = "Normal"
        };

        // Act
        var batchResponse = await _client.PostAsJsonAsync("/api/simulation/batch", batchRequest);
        batchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var batchResult = await batchResponse.Content.ReadFromJsonAsync<BatchSimulationResult>();
        batchResult.Should().NotBeNull();
        batchResult!.BatchId.Should().NotBeEmpty();
        batchResult.TotalSimulations.Should().Be(3);
        batchResult.Status.Should().Be("Processing");

        var batchId = batchResult.BatchId;

        // Act & Assert - Monitor batch progress
        await WaitForBatchProgress(batchId, 66); // Wait for at least 2/3 completion

        var statusResponse = await _client.GetAsync($"/api/simulation/batch/{batchId}/status");
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var statusResult = await statusResponse.Content.ReadFromJsonAsync<BatchSimulationStatus>();
        statusResult.Should().NotBeNull();
        statusResult!.CompletedSimulations.Should().BeGreaterOrEqualTo(2);
        statusResult.FailedSimulations.Should().Be(0); // Should have no failures

        // Act & Assert - Get batch results
        var resultsResponse = await _client.GetAsync($"/api/simulation/batch/{batchId}/results");
        resultsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var results = await resultsResponse.Content.ReadFromJsonAsync<BatchSimulationResults>();
        results.Should().NotBeNull();
        results!.Simulations.Should().HaveCount(3);
        
        // Verify all simulations completed successfully
        results.Simulations.Should().OnlyContain(s => s.Status == "Completed");
        results.Simulations.Should().OnlyContain(s => s.DataPoints.Count > 0);
    }

    #region Helper Methods

    private async Task<Guid> CreateTestMachine()
    {
        var machine = new
        {
            Name = "Simulation Test Machine " + DateTime.Now.Ticks,
            Type = "CNC",
            SerialNumber = "SIM-" + Guid.NewGuid().ToString("N")[..8],
            Manufacturer = "TestCorp",
            Model = "TC-1000",
            InstallationDate = DateTime.UtcNow.AddYears(-1),
            Location = "Simulation Lab",
            Criticality = 3,
            Status = "Operational"
        };

        var response = await _client.PostAsJsonAsync("/api/machines", machine);
        response.EnsureSuccessStatusCode();
        
        var createdMachine = await response.Content.ReadFromJsonAsync<MachineDto>();
        return createdMachine!.Id;
    }

    private async Task WaitForSimulationProgress(Guid simulationId, int minProgress)
    {
        var maxWaitTime = TimeSpan.FromSeconds(30);
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            var response = await _client.GetAsync($"/api/simulation/{simulationId}/status");
            if (response.IsSuccessStatusCode)
            {
                var status = await response.Content.ReadFromJsonAsync<SimulationStatus>();
                if (status?.ProgressPercentage >= minProgress)
                    return;
            }
            
            await Task.Delay(1000); // Check every second
        }

        throw new TimeoutException($"Simulation did not reach {minProgress}% progress within timeout period");
    }

    private async Task WaitForSimulationCompletion(Guid simulationId)
    {
        var maxWaitTime = TimeSpan.FromSeconds(45);
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            var response = await _client.GetAsync($"/api/simulation/{simulationId}/status");
            if (response.IsSuccessStatusCode)
            {
                var status = await response.Content.ReadFromJsonAsync<SimulationStatus>();
                if (status?.Status == "Completed" || status?.Status == "Failed")
                    return;
            }
            
            await Task.Delay(1500); // Check every 1.5 seconds
        }

        _output.WriteLine($"Warning: Simulation {simulationId} did not complete within timeout period");
    }

    private async Task WaitForBatchProgress(Guid batchId, int minProgress)
    {
        var maxWaitTime = TimeSpan.FromSeconds(60);
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            var response = await _client.GetAsync($"/api/simulation/batch/{batchId}/status");
            if (response.IsSuccessStatusCode)
            {
                var status = await response.Content.ReadFromJsonAsync<BatchSimulationStatus>();
                var progress = status?.CompletedSimulations * 100 / status?.TotalSimulations ?? 0;
                if (progress >= minProgress)
                    return;
            }
            
            await Task.Delay(2000); // Check every 2 seconds for batch
        }

        throw new TimeoutException($"Batch simulation did not reach {minProgress}% progress within timeout period");
    }

    private double CalculateTrend(double[] values)
    {
        if (values.Length < 2) return 0;

        var n = values.Length;
        var sumX = n * (n - 1) / 2.0;
        var sumY = values.Sum();
        var sumXY = values.Select((y, i) => y * i).Sum();
        var sumXX = Enumerable.Range(0, n).Select(i => i * i).Sum();

        var slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);
        return slope;
    }

    #endregion
}

#region DTO Classes for Simulation Testing

public class SimulationStartResponse
{
    public Guid SimulationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SimulationStatus
{
    public Guid SimulationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public double ProgressPercentage { get; set; }
    public DateTime? EstimatedCompletionTime { get; set; }
    public int DataPointsGenerated { get; set; }
}

public class SimulationResults
{
    public Guid SimulationId { get; set; }
    public Guid MachineId { get; set; }
    public List<SimulationDataPoint> DataPoints { get; set; } = [];
    public SimulationMetadata Metadata { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class SimulationDataPoint
{
    public DateTime Timestamp { get; set; }
    public double HealthIndicator { get; set; }
    public double Temperature { get; set; }
    public double Vibration { get; set; }
    public double? Pressure { get; set; }
    public double? RotationalSpeed { get; set; }
    public Dictionary<string, object> AdditionalParameters { get; set; } = [];
}

public class SimulationMetadata
{
    public string DegradationModel { get; set; } = string.Empty;
    public int TotalDataPoints { get; set; }
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> ModelParameters { get; set; } = [];
}

public class BatchSimulationResult
{
    public Guid BatchId { get; set; }
    public int TotalSimulations { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime QueuedAt { get; set; }
}

public class BatchSimulationStatus
{
    public Guid BatchId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalSimulations { get; set; }
    public int CompletedSimulations { get; set; }
    public int FailedSimulations { get; set; }
    public int RunningSimulations { get; set; }
    public double OverallProgress { get; set; }
}

public class BatchSimulationResults
{
    public Guid BatchId { get; set; }
    public List<BatchSimulationItem> Simulations { get; set; } = [];
    public DateTime CompletedAt { get; set; }
    public BatchSummary Summary { get; set; } = new();
}

public class BatchSimulationItem
{
    public Guid SimulationId { get; set; }
    public Guid MachineId { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<SimulationDataPoint> DataPoints { get; set; } = [];
    public string ErrorMessage { get; set; } = string.Empty;
}

public class BatchSummary
{
    public int SuccessfulSimulations { get; set; }
    public int FailedSimulations { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public double AverageDataPointsPerSimulation { get; set; }
}

#endregion