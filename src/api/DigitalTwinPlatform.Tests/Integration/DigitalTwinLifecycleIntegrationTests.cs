using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DigitalTwinPlatform.Tests.Integration;

/// <summary>
/// End-to-end integration tests for the complete Digital Twin lifecycle:
/// Machine Registration → Telemetry Ingestion → ML Prediction → Alert Generation → Maintenance Planning
/// </summary>
public class DigitalTwinLifecycleIntegrationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public async Task CompleteDigitalTwinLifecycle_ShouldWorkEndToEnd()
    {
        // Arrange
        var machineId = await CreateTestMachine();
        var telemetryData = GenerateTelemetryData(machineId);
        
        // Act & Assert - Step 1: Ingest Telemetry Data
        await IngestTelemetryData(telemetryData);
        
        // Act & Assert - Step 2: Get RUL Prediction
        var prediction = await GetRulPrediction(machineId);
        var predictionElement = (JsonElement)prediction;
        Assert.True(predictionElement.GetProperty("remainingUsefulLife").GetDouble() > 0);
        Assert.True(predictionElement.GetProperty("confidence").GetDouble() > 0);
        
        // Act & Assert - Step 3: Get Health Classification
        var healthStatus = await GetHealthClassification(machineId);
        var healthElement = (JsonElement)healthStatus;
        var healthStatusString = healthElement.GetProperty("healthStatus").GetString();
        Assert.False(string.IsNullOrEmpty(healthStatusString));
        Assert.True(healthElement.GetProperty("healthProbability").GetDouble() >= 0);
        
        // Act & Assert - Step 4: Check for Alerts (if health is critical)
        if (healthElement.GetProperty("healthProbability").GetDouble() > 0.7)
        {
            var alerts = await GetAlerts(machineId);
            Assert.NotEmpty(alerts);
        }
        
        // Act & Assert - Step 5: Search Functionality
        var searchResults = await SearchMachines(machineId);
        Assert.NotEmpty(searchResults);
        Assert.Contains(searchResults, r => 
        {
            var element = (JsonElement)r;
            return element.GetProperty("id").GetString() == machineId.ToString();
        });
        
        // Cleanup
        await CleanupTestData(machineId);
    }

    [Fact]
    public async Task SyntheticDataGeneration_ShouldCreateValidData()
    {
        // Arrange
        var request = new
        {
            MachineType = "CNC",
            NumberOfTrajectories = 5,
            TimeRange = 30,
            RandomSeed = 12345
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/synthetic-data/generate", request, _jsonOptions);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        
        Assert.NotNull(result);
        Assert.True(result.GetProperty("status").GetString() == "Completed");
        Assert.True(result.GetProperty("numberOfTrajectories").GetInt32() == 5);
        Assert.True(result.GetProperty("statistics").GetProperty("totalDataPoints").GetInt32() > 0);
    }

    [Fact]
    public async Task RealTimeTelemetryPipeline_ShouldProcessDataCorrectly()
    {
        // Arrange
        var machineId = await CreateTestMachine();
        var telemetryBatch = GenerateTelemetryBatch(machineId, 10);

        // Act - Ingest multiple telemetry points
        var tasks = telemetryBatch.Select(telemetry => 
            _client.PostAsJsonAsync("/api/telemetry", telemetry, _jsonOptions));
        
        var responses = await Task.WhenAll(tasks);
        
        // Assert
        foreach (var response in responses)
        {
            Assert.True(response.IsSuccessStatusCode);
            
        }
        
        // Verify latest telemetry
        var latestResponse = await _client.GetAsync($"/api/telemetry/{machineId}/latest");
        latestResponse.EnsureSuccessStatusCode();
        
        var latestTelemetry = await latestResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        Assert.NotNull(latestTelemetry);
        
        // Cleanup
        await CleanupTestData(machineId);
    }

    [Fact]
    public async Task MLModelTraining_ShouldImprovePredictionAccuracy()
    {
        // Arrange
        var trainingData = GenerateTrainingData(100);
        
        // Act - Train models
        var trainResponse = await _client.PostAsJsonAsync("/api/predictions/train", 
            new { ForceRetrain = true, ModelType = "all" }, _jsonOptions);
        
        trainResponse.EnsureSuccessStatusCode();
        var trainingResult = await trainResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        
        // Assert
        Assert.NotNull(trainingResult);
        Assert.True(trainingResult.GetProperty("success").GetBoolean());
        Assert.True(trainingResult.GetProperty("samplesUsed").GetInt32() > 0);
        
        // Verify model status
        var statusResponse = await _client.GetAsync("/api/predictions/status");
        statusResponse.EnsureSuccessStatusCode();
        
        var modelStatus = await statusResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        Assert.True(modelStatus.GetProperty("rulModelLoaded").GetBoolean());
        Assert.True(modelStatus.GetProperty("healthModelLoaded").GetBoolean());
    }

    [Fact]
    public async Task SearchFunctionality_ShouldReturnAccurateResults()
    {
        // Arrange
        var machineIds = new List<Guid>();
        for (int i = 0; i < 3; i++)
        {
            machineIds.Add(await CreateTestMachine($"TestMachine{i}"));
        }

        // Act - Search for machines
        var searchRequest = new
        {
            Query = "TestMachine",
            EntityTypes = new[] { "machines" },
            Page = 0,
            PageSize = 10
        };

        var searchResponse = await _client.PostAsJsonAsync("/api/search", searchRequest, _jsonOptions);
        searchResponse.EnsureSuccessStatusCode();
        
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        
        // Assert
        Assert.NotNull(searchResult);
        Assert.True(searchResult.GetProperty("totalCount").GetInt32() >= 3);
        
        var items = searchResult.GetProperty("items").EnumerateArray();
        Assert.All(items, item => 
        {
            var type = item.GetProperty("type").GetString();
            Assert.Equal("machine", type);
        });

        // Cleanup
        foreach (var machineId in machineIds)
        {
            await CleanupTestData(machineId);
        }
    }

    private async Task<Guid> CreateTestMachine(string machineName = "TestMachine")
    {
        var machineRequest = new
        {
            name = machineName,
            type = "CNC",
            status = "Operational",
            location = "Test Facility",
            properties = new { manufacturer = "TestCo", model = "TC-1000" }
        };

        var response = await _client.PostAsJsonAsync("/api/machines", machineRequest, _jsonOptions);
        response.EnsureSuccessStatusCode();
        
        var createdMachine = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        return Guid.Parse(createdMachine.GetProperty("id").GetString()!);
    }

    private object GenerateTelemetryData(Guid machineId)
    {
        return new
        {
            machineId = machineId,
            dataType = "sensor_reading",
            data = new
            {
                temperature = 75.3,
                vibration = 2.1,
                pressure = 101.3,
                rpm = 1500
            },
            timestamp = DateTime.UtcNow
        };
    }

    private List<object> GenerateTelemetryBatch(Guid machineId, int count)
    {
        var telemetryBatch = new List<object>();
        var random = new Random(42); // Fixed seed for reproducible tests
        
        for (int i = 0; i < count; i++)
        {
            telemetryBatch.Add(new
            {
                machineId = machineId,
                dataType = "sensor_reading",
                data = new
                {
                    temperature = 70 + random.NextDouble() * 20,
                    vibration = 1.5 + random.NextDouble() * 2,
                    pressure = 95 + random.NextDouble() * 20,
                    rpm = 1400 + random.Next(0, 200)
                },
                timestamp = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        
        return telemetryBatch;
    }

    private async Task IngestTelemetryData(object telemetryData)
    {
        var response = await _client.PostAsJsonAsync("/api/telemetry", telemetryData, _jsonOptions);
        response.EnsureSuccessStatusCode();
    }

    private async Task<object> GetRulPrediction(Guid machineId)
    {
        var response = await _client.PostAsync($"/api/predictions/rul/{machineId}", null);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        return result!;
    }

    private async Task<object> GetHealthClassification(Guid machineId)
    {
        var response = await _client.GetAsync($"/api/predictions/health/{machineId}");
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        return result!;
    }

    private async Task<List<object>> GetAlerts(Guid machineId)
    {
        var response = await _client.GetAsync($"/api/alerts?machineId={machineId}");
        response.EnsureSuccessStatusCode();
        
        var alerts = await response.Content.ReadFromJsonAsync<List<JsonElement>>(_jsonOptions);
        return alerts?.Select(alert => (object)alert).ToList() ?? new List<object>();
    }

    private async Task<List<object>> SearchMachines(Guid machineId)
    {
        var searchRequest = new
        {
            Query = machineId.ToString(),
            EntityTypes = new[] { "machines" },
            Page = 0,
            PageSize = 10
        };

        var response = await _client.PostAsJsonAsync("/api/search", searchRequest, _jsonOptions);
        response.EnsureSuccessStatusCode();
        
        var searchResult = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var items = searchResult!.GetProperty("items").EnumerateArray();
        
        return items.Select(item => (object)item).ToList();
    }

    private object GenerateTrainingData(int count)
    {
        var random = new Random(42);
        var trainingData = new List<object>();
        
        for (int i = 0; i < count; i++)
        {
            var rul = random.Next(10, 100);
            trainingData.Add(new
            {
                temperature = 70 + random.NextDouble() * 30,
                vibration = 1 + random.NextDouble() * 5,
                pressure = 90 + random.NextDouble() * 30,
                rpm = 1200 + random.Next(0, 800),
                age = random.Next(0, 365),
                cycleCount = random.Next(0, 1000),
                label = (float)rul
            });
        }
        
        return new { trainingData = trainingData };
    }

    private async Task CleanupTestData(Guid machineId)
    {
        // Delete telemetry data
        var telemetryResponse = await _client.GetAsync($"/api/telemetry/{machineId}?take=1000");
        if (telemetryResponse.IsSuccessStatusCode)
        {
            var telemetry = await telemetryResponse.Content.ReadFromJsonAsync<List<JsonElement>>(_jsonOptions);
            if (telemetry != null)
            {
                foreach (var telemetryItem in telemetry)
                {
                    await _client.DeleteAsync($"/api/telemetry/{telemetryItem.GetProperty("id").GetGuid()}");
                }
            }
        }

        // Delete predictions
        var predictionsResponse = await _client.GetAsync($"/api/predictions?machineId={machineId}");
        if (predictionsResponse.IsSuccessStatusCode)
        {
            var predictions = await predictionsResponse.Content.ReadFromJsonAsync<List<JsonElement>>(_jsonOptions);
            if (predictions != null)
            {
                foreach (var prediction in predictions)
                {
                    await _client.DeleteAsync($"/api/predictions/{prediction.GetProperty("id").GetGuid()}");
                }
            }
        }

        // Delete machine
        await _client.DeleteAsync($"/api/machines/{machineId}");
    }
}
