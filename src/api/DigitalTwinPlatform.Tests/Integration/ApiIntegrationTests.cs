using System.Net;
using System.Net.Http.Json;
using DigitalTwinPlatform.Application.Predictions.Models;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Entities.Enums;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinPlatform.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Sequential")] // Run tests sequentially to avoid conflicts
public class ApiIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly IntegrationTestFixture _fixture;

    public ApiIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task Health_Endpoint_Returns_OK()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task Machines_CRUD_Operations_Work_Correctly()
    {
        // Arrange
        var testMachine = new
        {
            Name = "Test Machine Integration",
            Type = "CNC",
            SerialNumber = "TEST-INT-" + Guid.NewGuid().ToString("N")[..8],
            Manufacturer = "TestCorp",
            Model = "TC-1000",
            InstallationDate = DateTime.UtcNow.AddYears(-1),
            Location = "Test Facility",
            Criticality = 3,
            Status = "Operational"
        };

        // Act & Assert - Create
        var createResponse = await _client.PostAsJsonAsync("/api/machines", testMachine);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var createdMachine = await createResponse.Content.ReadFromJsonAsync<MachineDto>();
        createdMachine.Should().NotBeNull();
        createdMachine!.Name.Should().Be(testMachine.Name);
        createdMachine.SerialNumber.Should().Be(testMachine.SerialNumber);

        // Act & Assert - Read All
        var getAllResponse = await _client.GetAsync("/api/machines");
        getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var allMachines = await getAllResponse.Content.ReadFromJsonAsync<List<MachineDto>>();
        allMachines.Should().NotBeNull();
        allMachines!.Should().Contain(m => m.Id == createdMachine.Id);

        // Act & Assert - Read Single
        var getSingleResponse = await _client.GetAsync($"/api/machines/{createdMachine.Id}");
        getSingleResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var retrievedMachine = await getSingleResponse.Content.ReadFromJsonAsync<MachineDto>();
        retrievedMachine.Should().NotBeNull();
        retrievedMachine!.Id.Should().Be(createdMachine.Id);

        // Act & Assert - Update
        var updatedMachine = new
        {
            Id = createdMachine.Id,
            Name = "Updated Test Machine",
            Type = testMachine.Type,
            SerialNumber = testMachine.SerialNumber,
            Manufacturer = testMachine.Manufacturer,
            Model = testMachine.Model,
            InstallationDate = testMachine.InstallationDate,
            Location = testMachine.Location,
            Criticality = 4, // Increased criticality
            Status = "Maintenance"
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/machines/{createdMachine.Id}", updatedMachine);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getUpdatedResponse = await _client.GetAsync($"/api/machines/{createdMachine.Id}");
        var updatedResult = await getUpdatedResponse.Content.ReadFromJsonAsync<MachineDto>();
        updatedResult.Should().NotBeNull();
        updatedResult!.Criticality.Should().Be(4);
        updatedResult.Status.Should().Be("Maintenance");

        // Act & Assert - Delete
        var deleteResponse = await _client.DeleteAsync($"/api/machines/{createdMachine.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getDeletedResponse = await _client.GetAsync($"/api/machines/{createdMachine.Id}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Telemetry_Ingestion_And_Querying_Work_Correctly()
    {
        // Arrange
        var machineId = await CreateTestMachine();
        
        var telemetryData = new
        {
            MachineId = machineId,
            Timestamp = DateTime.UtcNow,
            Temperature = 72.5,
            Vibration = 0.3,
            Pressure = 45.2,
            RotationalSpeed = 1200
        };

        // Act & Assert - Ingest
        var ingestResponse = await _client.PostAsJsonAsync("/api/telemetry", telemetryData);
        ingestResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Allow time for processing
        await Task.Delay(1000);

        // Act & Assert - Query recent telemetry
        var queryResponse = await _client.GetAsync($"/api/telemetry/machine/{machineId}?take=10");
        queryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var telemetryRecords = await queryResponse.Content.ReadFromJsonAsync<List<TelemetryDto>>();
        telemetryRecords.Should().NotBeNull();
        telemetryRecords!.Should().NotBeEmpty();
        telemetryRecords.First().MachineId.Should().Be(machineId);
        telemetryRecords.First().Temperature.Should().Be(72.5);
    }

    [Fact]
    public async Task Predictions_Endpoint_Generates_Valid_Predictions()
    {
        // Arrange
        var machineId = await CreateTestMachine();
        
        // First, add some telemetry data
        await GenerateTelemetryData(machineId, 50);

        // Act
        var predictionResponse = await _client.PostAsJsonAsync("/api/predictions", new { MachineId = machineId });
        
        // Assert
        predictionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var prediction = await predictionResponse.Content.ReadFromJsonAsync<PredictionDto>();
        prediction.Should().NotBeNull();
        prediction!.MachineId.Should().Be(machineId);
        prediction.RemainingUsefulLifeDays.Should().BeGreaterThan(0);
        prediction.FailureProbability.Should().BeGreaterOrEqualTo(0).And.BeLessOrEqualTo(1);
        prediction.HealthStatus.Should().NotBe(HealthClassification.Healthy);
    }

    [Fact]
    public async Task Uncertainty_Analysis_Endpoints_Work_Correctly()
    {
        // Arrange
        var machineId = await CreateTestMachine();
        
        var monteCarloRequest = new
        {
            BaseFeatures = new Dictionary<string, double>
            {
                ["temperature"] = 72.5,
                ["vibration"] = 0.3,
                ["pressure"] = 45.2
            },
            Iterations = 100,
            NoiseLevel = 0.1
        };

        // Act & Assert - Monte Carlo Analysis
        var monteCarloResponse = await _client.PostAsJsonAsync($"/api/uncertainty/{machineId}/monte-carlo", monteCarloRequest);
        monteCarloResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var monteCarloResult = await monteCarloResponse.Content.ReadFromJsonAsync<UncertaintyAnalysisResponse>();
        monteCarloResult.Should().NotBeNull();
        monteCarloResult!.MeanPrediction.Should().BeGreaterThan(0);
        monteCarloResult.StandardDeviation.Should().BeGreaterOrEqualTo(0);

        // Act & Assert - Bootstrap Intervals
        var bootstrapResponse = await _client.GetAsync($"/api/uncertainty/{machineId}/bootstrap-intervals?samples=50&confidenceLevel=0.95");
        bootstrapResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var bootstrapResult = await bootstrapResponse.Content.ReadFromJsonAsync<ConfidenceIntervalDto>();
        bootstrapResult.Should().NotBeNull();
        bootstrapResult!.LowerBound.Should().BeLessThan(bootstrapResult.UpperBound);
    }

    [Fact]
    public async Task Mathematical_Modeling_Endpoints_Function_Properly()
    {
        // Arrange
        var odeRequest = new
        {
            SystemName = "Test Spring Mass Damper",
            Equations = new[] { "dv/dt = -k*x/m - c*v/m", "dx/dt = v" },
            InitialConditions = new[] { 0.0, 1.0 }, // v=0, x=1
            StartTime = 0.0,
            EndTime = 10.0,
            StepSize = 0.01
        };

        // Act & Assert - ODE Solving
        var odeResponse = await _client.PostAsJsonAsync("/api/mathematicalmodeling/ode/solve", odeRequest);
        odeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var odeResult = await odeResponse.Content.ReadFromJsonAsync<OdeSolutionDto>();
        odeResult.Should().NotBeNull();
        odeResult!.SystemName.Should().Be("Test Spring Mass Damper");
        odeResult.Solutions.Should().NotBeEmpty();
        odeResult.Steps.Should().BeGreaterThan(0);

        // Act & Assert - System Models Catalog
        var modelsResponse = await _client.GetAsync("/api/mathematicalmodeling/models");
        modelsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var modelsResult = await modelsResponse.Content.ReadFromJsonAsync<SystemModelCatalogDto>();
        modelsResult.Should().NotBeNull();
        modelsResult!.Models.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Advanced_Analytics_Dashboard_Works_Correctly()
    {
        // Arrange
        var machineId = await CreateTestMachine();
        
        // Add telemetry data for analysis
        await GenerateTelemetryData(machineId, 100);

        // Act
        var dashboardResponse = await _client.GetAsync($"/api/advancedanalytics/dashboard/{machineId}");
        
        // Assert
        dashboardResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dashboard = await dashboardResponse.Content.ReadFromJsonAsync<AdvancedAnalyticsDashboard>();
        dashboard.Should().NotBeNull();
        dashboard!.MachineId.Should().Be(machineId);
        dashboard.Prediction.Should().NotBeNull();
        dashboard.AnomalyDetection.Should().NotBeNull();
        dashboard.MaintenanceRecommendation.Should().NotBeNull();
        dashboard.HealthScore.Should().BeGreaterOrEqualTo(0).And.BeLessOrEqualTo(1);
    }

    [Fact]
    public async Task Authentication_Required_For_Protected_Endpoints()
    {
        // Arrange
        var client = _fixture.CreateClient(); // Fresh client without auth
        client.DefaultRequestHeaders.Clear(); // Remove any auth headers

        // Act
        var response = await client.GetAsync("/api/machines");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Error_Handling_Returns_Proper_Error_Formats()
    {
        // Act
        var response = await _client.GetAsync("/api/nonexistent-endpoint");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorDto>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Status.Should().Be((int)HttpStatusCode.NotFound);
        errorResponse.Title.Should().NotBeNullOrEmpty();
    }

    #region Helper Methods

    private async Task<Guid> CreateTestMachine()
    {
        var machine = new
        {
            Name = "Integration Test Machine " + DateTime.Now.Ticks,
            Type = "TestType",
            SerialNumber = "INT-" + Guid.NewGuid().ToString("N")[..8],
            Manufacturer = "TestCorp",
            Model = "TC-1000",
            InstallationDate = DateTime.UtcNow.AddYears(-1),
            Location = "Test Facility",
            Criticality = 3,
            Status = "Operational"
        };

        var response = await _client.PostAsJsonAsync("/api/machines", machine);
        response.EnsureSuccessStatusCode();
        
        var createdMachine = await response.Content.ReadFromJsonAsync<MachineDto>();
        return createdMachine!.Id;
    }

    private async Task GenerateTelemetryData(Guid machineId, int count)
    {
        var random = new Random();
        
        for (int i = 0; i < count; i++)
        {
            var telemetry = new
            {
                MachineId = machineId,
                Timestamp = DateTime.UtcNow.AddMinutes(-count + i),
                Temperature = 65 + random.NextDouble() * 20, // 65-85°F
                Vibration = 0.1 + random.NextDouble() * 0.5, // 0.1-0.6
                Pressure = 40 + random.NextDouble() * 20, // 40-60 PSI
                RotationalSpeed = 1000 + random.Next(0, 500) // 1000-1500 RPM
            };

            await _client.PostAsJsonAsync("/api/telemetry", telemetry);
        }
        
        // Allow time for all data to be processed
        await Task.Delay(500);
    }

    #endregion
}

#region DTO Classes for Testing

public class MachineDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTime InstallationDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public int Criticality { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class TelemetryDto
{
    public Guid Id { get; set; }
    public Guid MachineId { get; set; }
    public DateTime Timestamp { get; set; }
    public double Temperature { get; set; }
    public double Vibration { get; set; }
    public double? Pressure { get; set; }
    public double? RotationalSpeed { get; set; }
}

public class PredictionDto
{
    public Guid Id { get; set; }
    public Guid MachineId { get; set; }
    public double RemainingUsefulLifeDays { get; set; }
    public double RulLowerBound { get; set; }
    public double RulUpperBound { get; set; }
    public double FailureProbability { get; set; }
    public HealthClassification HealthStatus { get; set; }
    public Dictionary<string, double> FeatureContributions { get; set; } = [];
    public DateTime Timestamp { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
}

public class UncertaintyAnalysisResponse
{
    public string AnalysisType { get; set; } = string.Empty;
    public Guid MachineId { get; set; }
    public double MeanPrediction { get; set; }
    public double StandardDeviation { get; set; }
    public ConfidenceIntervalDto ConfidenceInterval { get; set; } = new();
    public Dictionary<string, double> FeatureUncertainties { get; set; } = [];
    public double PredictionVariance { get; set; }
    public int SampleCount { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ConfidenceIntervalDto
{
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public double ConfidenceLevel { get; set; }
    public double? CoverageProbability { get; set; }
    public DateTime Timestamp { get; set; }
}

public class OdeSolutionDto
{
    public string SystemName { get; set; } = string.Empty;
    public double[] TimePoints { get; set; } = [];
    public double[][] Solutions { get; set; } = [];
    public int Steps { get; set; }
    public double FinalTime { get; set; }
    public double[] FinalState { get; set; } = [];
}

public class SystemModelCatalogDto
{
    public SystemModelDto[] Models { get; set; } = [];
}

public class SystemModelDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Equations { get; set; } = [];
    public string[] Parameters { get; set; } = [];
    public string[] Variables { get; set; } = [];
}

public class AdvancedAnalyticsDashboard
{
    public Guid MachineId { get; set; }
    public PredictionDto Prediction { get; set; } = new();
    public AnomalyDetectionResultDto AnomalyDetection { get; set; } = new();
    public PrescriptiveRecommendationDto MaintenanceRecommendation { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public double HealthScore { get; set; }
}

public class AnomalyDetectionResultDto
{
    public Guid MachineId { get; set; }
    public int TotalAnomalies { get; set; }
    public double OverallRiskScore { get; set; }
}

public class PrescriptiveRecommendationDto
{
    public Guid MachineId { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
    public double PriorityScore { get; set; }
    public DateTime RecommendedTiming { get; set; }
}

public class ApiErrorDto
{
    public int Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
}

#endregion