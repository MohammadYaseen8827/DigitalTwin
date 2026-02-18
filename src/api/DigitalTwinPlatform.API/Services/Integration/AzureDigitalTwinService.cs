using Azure;
using Azure.DigitalTwins.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Application.Abstractions.Repositories;

namespace DigitalTwinPlatform.API.Services.Integration;

public class AzureDigitalTwinService : IAzureDigitalTwinService
{
    private readonly DigitalTwinsClient? _client;
    private readonly IMachineRepository _machineRepository;
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly IPredictionRepository _predictionRepository;
    private readonly ILogger<AzureDigitalTwinService> _logger;

    public AzureDigitalTwinService(
        DigitalTwinsClient? client,
        IMachineRepository machineRepository,
        ITelemetryRepository telemetryRepository,
        IPredictionRepository predictionRepository,
        ILogger<AzureDigitalTwinService> logger)
    {
        _client = client;
        _machineRepository = machineRepository;
        _telemetryRepository = telemetryRepository;
        _predictionRepository = predictionRepository;
        _logger = logger;
    
        if (_client == null)
        {
            _logger.LogWarning("Azure Digital Twins client is not configured. Digital twin features will be disabled.");
        }
    }

    public async Task SyncMachineAsync(Guid machineId)
    {
        if (_client == null) 
        {
            _logger.LogWarning("Azure Digital Twins client not available. Skipping machine sync.");
            return;
        }

        try
        {
            var twinId = machineId.ToString();
            var machine = await _machineRepository.GetAsync(machineId);
            
            if (machine == null)
            {
                _logger.LogWarning("Machine {MachineId} not found for Azure Digital Twins sync", machineId);
                return;
            }

            // Map machine properties to DTDL format
            var twinData = await MapMachineToDtdlAsync(machine);
            
            await _client.CreateOrReplaceDigitalTwinAsync(twinId, twinData);
            
            _logger.LogInformation("Successfully synced machine {MachineId} to Azure Digital Twins", machineId);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to sync machine {MachineId} to Azure Digital Twins", machineId);
        }
    }

    public async Task UpsertTwinAsync(Guid id, string modelId, IDictionary<string, object>? properties = null)
    {
        if (_client == null) 
        {
            _logger.LogWarning("Azure Digital Twins client not available. Skipping twin upsert.");
            return;
        }

        try
        {
            var twinId = id.ToString();
            var payload = new Dictionary<string, object>
            {
                ["$metadata"] = new { model = modelId }
            };

            // Add provided properties
            if (properties != null)
            {
                foreach (var kvp in properties)
                    payload[kvp.Key] = kvp.Value;
            }

            // Add standard properties
            payload["lastSyncedAt"] = DateTime.UtcNow;
            payload["syncStatus"] = "active";

            await _client.CreateOrReplaceDigitalTwinAsync(twinId, payload);
            
            _logger.LogDebug("Successfully upserted twin {TwinId} with model {ModelId}", twinId, modelId);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to upsert twin {TwinId} with model {ModelId}", id, modelId);
        }
    }

    public async Task SyncProductionLineAsync(Guid productionLineId)
    {
        if (_client == null) 
        {
            _logger.LogWarning("Azure Digital Twins client not available. Skipping production line sync.");
            return;
        }

        try
        {
            var twinId = productionLineId.ToString();
            
            // Map production line to DTDL format
            var twinData = new Dictionary<string, object>
            {
                ["$metadata"] = new { model = "dtmi:com:example:ProductionLine;1" },
                ["name"] = $"ProductionLine-{productionLineId:N}",
                ["type"] = "production_line",
                ["status"] = "operational",
                ["lastSyncedAt"] = DateTime.UtcNow,
                ["machineCount"] = 0, // Will be updated when machines are synced
                ["operationalEfficiency"] = 0.95,
                ["location"] = new
                {
                    facility = "MainFactory",
                    zone = "ProductionZoneA"
                }
            };

            await _client.CreateOrReplaceDigitalTwinAsync(twinId, twinData);
            
            _logger.LogInformation("Successfully synced production line {ProductionLineId} to Azure Digital Twins", productionLineId);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to sync production line {ProductionLineId} to Azure Digital Twins", productionLineId);
        }
    }

    private async Task<Dictionary<string, object>> MapMachineToDtdlAsync(Machine machine)
    {
        var twinData = new Dictionary<string, object>
        {
            ["$metadata"] = new { model = GetMachineModelId(machine.Type.Value) },
            ["name"] = machine.Name.Value,
            ["type"] = machine.Type.Value.ToLower(),
            ["status"] = machine.IsActive ? "active" : "inactive",
            ["lastSyncedAt"] = DateTime.UtcNow,
            ["location"] = new
            {
                facility =  "default",
                coordinates = new
                {
                    x = 0.0,
                    y = 0.0,
                    z = 0.0
                }
            }
        };

        // Add health status if available
        if (machine.HealthStatus.HasValue)
        {
            twinData["health"] = new
            {
                status = machine.HealthStatus.Value.ToString().ToLower(),
                classification = machine.HealthStatus.Value.ToString(),
                lastUpdated = DateTime.UtcNow
            };
        }

        // Add RUL if available
        if (machine.RemainingUsefulLifeDays.HasValue)
        {
            twinData["remainingUsefulLife"] = new
            {
                days = machine.RemainingUsefulLifeDays.Value,
                estimatedFailureDate = DateTime.UtcNow.AddDays(machine.RemainingUsefulLifeDays.Value),
                confidence = 0.85
            };
        }

        // Add recent telemetry summary
        var recentTelemetry = (await _telemetryRepository.GetForMachineAsync(machine.Id, null, 10)).ToList();
        if (recentTelemetry.Count != 0)
        {
            twinData["telemetry"] = MapTelemetryToDtdl(recentTelemetry);
        }

        // Add latest prediction if available
        var latestPrediction = (await _predictionRepository.GetAllAsync(p => p.MachineId == machine.Id))
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault();

        if (latestPrediction != null)
        {
            twinData["prediction"] = new
            {
                remainingUsefulLifeDays = latestPrediction.RemainingUsefulLifeDays,
                failureProbability = latestPrediction.FailureProbability,
                healthStatus = latestPrediction.HealthStatus.ToString(),
                modelVersion = latestPrediction.ModelVersion,
                createdAt = latestPrediction.CreatedAt
            };
        }

        // Add operational parameters based on machine type
        twinData["operationalParameters"] = GetOperationalParameters(machine.Type.Value);

        return twinData;
    }

    private static string GetMachineModelId(string? machineType)
    {
        return machineType?.ToLower() switch
        {
            "pump" => "dtmi:com:example:Pump;1",
            "motor" => "dtmi:com:example:Motor;1",
            "compressor" => "dtmi:com:example:Compressor;1",
            "valve" => "dtmi:com:example:Valve;1",
            "sensor" => "dtmi:com:example:Sensor;1",
            "conveyor" => "dtmi:com:example:Conveyor;1",
            _ => "dtmi:com:example:Machine;1"
        };
    }

    private static object MapTelemetryToDtdl(IEnumerable<TelemetryData> telemetry)
    {
        var telemetryList = telemetry.ToList();
        var telemetryByType = telemetryList.GroupBy(t => t.DataType).ToDictionary(g => g.Key, g => g.ToList());

        var result = new Dictionary<string, object>
        {
            ["lastUpdated"] = telemetryList.Max(t => t.Timestamp),
            ["dataPoints"] = telemetryList.Count
        };

        foreach (var (dataType, dataPoints) in telemetryByType)
        {
            var latestDataPoint = dataPoints.OrderByDescending(d => d.Timestamp).First();
            
            result[dataType] = new
            {
                currentValue = GetLatestValue(latestDataPoint),
                unit = GetUnitForDataType(dataType),
                timestamp = latestDataPoint.Timestamp,
                status = GetTelemetryStatus(latestDataPoint, dataType)
            };

            // Add statistical summary if we have multiple points
            if (dataPoints.Count > 1)
            {
                var numericValues = ExtractNumericValues(dataPoints);
                if (numericValues.Any())
                {
                    result[$"{dataType}_statistics"] = new
                    {
                        average = numericValues.Average(),
                        min = numericValues.Min(),
                        max = numericValues.Max(),
                        standardDeviation = CalculateStandardDeviation(numericValues)
                    };
                }
            }
        }

        return result;
    }

    private static object GetLatestValue(TelemetryData telemetry)
    {
        return telemetry.DataType.ToLower() switch
        {
            "temperature" => TryGetDouble(telemetry.Data, "value", out var tempVal) ? tempVal : 0,
            "vibration" => TryGetDouble(telemetry.Data, "amplitude", out var vibVal) ? vibVal : 0,
            "pressure" => TryGetDouble(telemetry.Data, "value", out var pressVal) ? pressVal : 0,
            "flow" => TryGetDouble(telemetry.Data, "rate", out var flowVal) ? flowVal : 0,
            "power" => TryGetDouble(telemetry.Data, "watts", out var powerVal) ? powerVal : 0,
            _ => telemetry.Data
        };
    }

    private static string GetUnitForDataType(string dataType)
    {
        return dataType.ToLower() switch
        {
            "temperature" => "°C",
            "vibration" => "mm/s",
            "pressure" => "bar",
            "flow" => "L/min",
            "power" => "kW",
            "speed" => "RPM",
            _ => "unit"
        };
    }

    private static string GetTelemetryStatus(TelemetryData telemetry, string dataType)
    {
        var value = GetLatestValue(telemetry);
        
        if (value is double numericValue)
        {
            return dataType.ToLower() switch
            {
                "temperature" => numericValue > 80 ? "high" : numericValue < 20 ? "low" : "normal",
                "vibration" => numericValue > 5 ? "high" : numericValue < 0.5 ? "low" : "normal",
                "pressure" => numericValue > 10 ? "high" : numericValue < 1 ? "low" : "normal",
                _ => "normal"
            };
        }

        return "unknown";
    }

    private static List<double> ExtractNumericValues(IEnumerable<TelemetryData> telemetryData)
    {
        var values = new List<double>();
        
        foreach (var telemetry in telemetryData)
        {
            var value = GetLatestValue(telemetry);
            if (value is double doubleValue)
            {
                values.Add(doubleValue);
            }
            else if (value is int intValue)
            {
                values.Add(intValue);
            }
            else if (value is decimal decimalValue)
            {
                values.Add(Convert.ToDouble(decimalValue));
            }
        }

        return values;
    }

    private static bool TryGetDouble(JsonDocument doc, string propertyName, out double value)
    {
        value = 0;
        if (doc.RootElement.ValueKind != JsonValueKind.Object) return false;
        if (!doc.RootElement.TryGetProperty(propertyName, out var element)) return false;
        return element.TryGetDouble(out value);
    }

    private static double CalculateStandardDeviation(IEnumerable<double> values)
    {
        var valueList = values.ToList();
        if (!valueList.Any()) return 0;

        var mean = valueList.Average();
        var sumOfSquares = valueList.Sum(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(sumOfSquares / valueList.Count);
    }

    private static object GetOperationalParameters(string? machineType)
    {
        return machineType?.ToLower() switch
        {
            "pump" => new
            {
                flowRate = new { min = 0, max = 100, unit = "L/min" },
                pressure = new { min = 0, max = 10, unit = "bar" },
                efficiency = new { nominal = 0.85, unit = "%" }
            },
            "motor" => new
            {
                speed = new { min = 0, max = 3000, unit = "RPM" },
                power = new { min = 0, max = 100, unit = "kW" },
                torque = new { min = 0, max = 500, unit = "Nm" }
            },
            "compressor" => new
            {
                pressure = new { min = 0, max = 15, unit = "bar" },
                flowRate = new { min = 0, max = 50, unit = "m³/min" },
                power = new { min = 0, max = 200, unit = "kW" }
            },
            _ => new
            {
                status = "operational",
                efficiency = new { nominal = 0.90, unit = "%" }
            }
        };
    }

    public async Task<bool> ValidateTwinConnectionAsync()
    {
        if (_client == null) return false;

        try
        {
            // Try to query a simple property to test connection
            var query = _client.QueryAsync<BasicDigitalTwin>("SELECT TOP 1 $dtId FROM DIGITALTWINS");
            var result = new List<BasicDigitalTwin>();
            await foreach (var item in query)
            {
                result.Add(item);
                break; // Only need one result to test connection
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure Digital Twins connection validation failed");
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetTwinModelsAsync()
    {
        if (_client == null) return Enumerable.Empty<string>();

        try
        {
            var models = _client.GetModelsAsync();
            var modelList = new List<string>();
            
            await foreach (var model in models)
            {
                modelList.Add(model.Id);
            }

            return modelList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Azure Digital Twins models");
            return Enumerable.Empty<string>();
        }
    }
}

// Helper class for basic twin queries
public class BasicDigitalTwin
{
    [JsonPropertyName("$dtId")]
    public string DtId { get; set; } = string.Empty;
}

