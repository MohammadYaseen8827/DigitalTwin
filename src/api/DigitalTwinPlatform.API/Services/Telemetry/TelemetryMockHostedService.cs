using System.Text.Json;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DigitalTwinPlatform.API.Services.Telemetry;

public class TelemetryMockHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<TelemetryMockHostedService> logger)
    : BackgroundService
{
    private static readonly string[] TelemetryTypes =
    [
        "temperature",
        "vibration",
        "pressure",
        "power_consumption",
        "production_count"
    ];

    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);
    private const int BatchSize = 100; // Process 100 records at a time
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var machineRepository = scope.ServiceProvider.GetRequiredService<IMachineRepository>();
                var telemetryRepository = scope.ServiceProvider.GetRequiredService<ITelemetryRepository>();

                var machines = (await machineRepository.GetAllAsync(ct: stoppingToken)).ToList();
                if (machines.Count == 0)
                {
                    logger.LogWarning("TelemetryMockHostedService found no machines to seed telemetry for.");
                }
                
                var telemetryData = new List<TelemetryData>();
                foreach (var machine in machines)
                {
                    var machineTelemetries = (CreateTelemetryEntries(machine.Id)).ToList();
                    telemetryData.AddRange(machineTelemetries);
                    logger.LogInformation(
                        "Mock telemetry generated for machine {machineName}: {Count} records",
                        machine.Name,
                        machineTelemetries.Count);
                }
                
                if (telemetryData.Count == 0)
                {
                    continue;
                }

                try
                {
                    // Process in batches
                for (int i = 0; i < telemetryData.Count; i += BatchSize)
                {
                    var batch = telemetryData.Skip(i).Take(BatchSize).ToList();
                    try
                    {
                        await telemetryRepository.AddRangeAsync(batch, stoppingToken);
                        logger.LogInformation(
                            "Mock telemetry batch {BatchNumber}  {Count} records",
                            (i / BatchSize) + 1,
                            batch.Count);
                    }
                    catch (Exception ex) when (ex is DbUpdateConcurrencyException || ex is DbUpdateException)
                    {
                        logger.LogWarning(ex,
                            "TelemetryMockHostedService error when inserting telemetry batch {BatchNumber}  Batch skipped.",
                            (i / BatchSize) + 1);
                        // Continue with next batch even if one fails
                    }
                }

                    await telemetryRepository.AddRangeAsync(telemetryData, stoppingToken);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    logger.LogWarning(ex,
                        "TelemetryMockHostedService concurrency conflict when inserting telemetry for tenant. Batch skipped.");
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when hosting shuts down
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "TelemetryMockHostedService iteration failed");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
            }
        }
    }

    private static IEnumerable<TelemetryData> CreateTelemetryEntries(Guid machineId)
    {
        var now = DateTime.UtcNow;
        foreach (var dataType in TelemetryTypes)
        {
            yield return new TelemetryData
            {
                Id = Guid.NewGuid(),
                MachineId = machineId,
                DataType = dataType,
                Data = CreatePayload(dataType),
                Timestamp = now
            };
        }
    }

    private static JsonDocument CreatePayload(string dataType)
    {
        var random = Random.Shared;

        object payload = dataType switch
        {
            "temperature" => new
            {
                value = Math.Round(60 + random.NextDouble() * 40, 2),
                unit = "C",
                minThreshold = 20,
                maxThreshold = 100
            },
            "vibration" => new
            {
                value = Math.Round(0.5 + random.NextDouble() * 3, 3),
                unit = "mm/s",
                frequency = 50,
                direction = "vertical"
            },
            "pressure" => new
            {
                value = Math.Round(40 + random.NextDouble() * 40, 2),
                unit = "bar",
                minThreshold = 20,
                maxThreshold = 120
            },
            "power_consumption" => new
            {
                value = Math.Round(5 + random.NextDouble() * 20, 2),
                unit = "kW",
                minThreshold = 2,
                maxThreshold = 30
            },
            "production_count" => new
            {
                value = Math.Round(80 + random.NextDouble() * 40, 2),
                target = 100,
                defects = (int)Math.Floor(random.NextDouble() * 5)
            },
            _ => new
            {
                value = Math.Round(80 + random.NextDouble() * 40, 2)
            }
        };

        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        return JsonDocument.Parse(bytes);
    }
}
