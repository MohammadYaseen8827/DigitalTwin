using System.Net;
using System.Net.Http.Json;
using DigitalTwinPlatform.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace DigitalTwinPlatform.Tests.Integration;

/// <summary>
/// Simplified integration tests focusing on core functionality
/// </summary>
[Trait("Category", "Integration")]
public class CoreIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFixture _fixture;

    public CoreIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Health_Endpoint_Returns_Success()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var health = await response.Content.ReadFromJsonAsync<object>();
        health.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_And_Get_Machine_Works()
    {
        // Arrange
        var testMachine = new
        {
            Name = "Integration Test Machine",
            Type = "CNC",
            SerialNumber = "INT-TEST-" + Guid.NewGuid().ToString("N")[..8],
            Manufacturer = "TestCorp",
            Model = "TC-1000",
            InstallationDate = DateTime.UtcNow.AddYears(-1),
            Location = "Test Facility",
            Criticality = 3,
            Status = "Operational"
        };

        // Act - Create
        var createResponse = await _client.PostAsJsonAsync("/api/machines", testMachine);
        
        // Assert - Create
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdMachine = await createResponse.Content.ReadFromJsonAsync<dynamic>();
        ((object)createdMachine!).Should().NotBeNull();

        // Act - Get
        var getResponse = await _client.GetAsync($"/api/machines/{createdMachine!.Id}");

        // Assert - Get
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedMachine = await getResponse.Content.ReadFromJsonAsync<dynamic>();
        ((string)retrievedMachine!.Name).Should().Be(testMachine.Name);
    }

    [Fact]
    public async Task Telemetry_Ingestion_And_Retrieval_Works()
    {
        // Arrange - Create machine first
        var machine = await CreateTestMachine();
        
        var telemetry = new
        {
            MachineId = machine.Id,
            DataType = "temperature",
            Data = new { value = 75.5, unit = "C" }
        };

        // Act - Ingest
        var ingestResponse = await _client.PostAsJsonAsync("/api/telemetry", telemetry);

        // Assert - Ingest
        ingestResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Allow time for processing
        await Task.Delay(1000);

        // Act - Retrieve
        var getResponse = await _client.GetAsync($"/api/telemetry/machine/{machine.Id}?take=10");

        // Assert - Retrieve
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var telemetryData = await getResponse.Content.ReadFromJsonAsync<dynamic[]>();
        telemetryData.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Basic_Prediction_Endpoint_Works()
    {
        // Arrange - Create machine and add some telemetry
        var machine = await CreateTestMachine();
        await GenerateTestData(machine.Id, 10);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/predictions/{machine.Id}", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var prediction = await response.Content.ReadFromJsonAsync<dynamic>();
        ((object)prediction!).Should().NotBeNull();
    }

    [Fact]
    public async Task Uncertainty_Analysis_Endpoint_Works()
    {
        // Arrange - Create machine and add some telemetry
        var machine = await CreateTestMachine();
        await GenerateTestData(machine.Id, 20);

        // Act
        var response = await _client.PostAsync($"/api/uncertainty/{machine.Id}/analyze", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var uncertainty = await response.Content.ReadFromJsonAsync<dynamic>();
        ((object)uncertainty!).Should().NotBeNull();
    }

    [Fact]
    public async Task Data_Drift_Detection_Works()
    {
        // Arrange - Create machine and add some telemetry
        var machine = await CreateTestMachine();
        await GenerateTestData(machine.Id, 30);

        // Act
        var response = await _client.GetAsync($"/api/datadrift/{machine.Id}/detect");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var driftResult = await response.Content.ReadFromJsonAsync<dynamic>();
        ((object)driftResult!).Should().NotBeNull();
    }

    #region Helper Methods

    private async Task<dynamic> CreateTestMachine()
    {
        var testMachine = new
        {
            Name = $"Test Machine {Guid.NewGuid():N}",
            Type = "CNC",
            SerialNumber = $"SN-{Guid.NewGuid():N}",
            Manufacturer = "TestCorp",
            Model = "TC-1000",
            InstallationDate = DateTime.UtcNow.AddYears(-1),
            Location = "Test Facility",
            Criticality = 3,
            Status = "Operational"
        };

        var response = await _client.PostAsJsonAsync("/api/machines", testMachine);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<dynamic>()!;
    }

    private async Task GenerateTestData(Guid machineId, int count)
    {
        var random = new Random();
        
        for (int i = 0; i < count; i++)
        {
            var telemetry = new
            {
                MachineId = machineId,
                DataType = "temperature",
                Data = new { value = 65 + random.NextDouble() * 20 }
            };

            await _client.PostAsJsonAsync("/api/telemetry", telemetry);
        }
        
        // Allow time for all data to be processed
        await Task.Delay(500);
    }

    #endregion
}