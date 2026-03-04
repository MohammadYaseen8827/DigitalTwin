using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.ValueObjects;
using System.Text.Json;
using DigitalTwinPlatform.Domain.Entities.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalTwinPlatform.Infrastructure.Persistence.SeedData;

public class DemoDataSeeder(IServiceProvider serviceProvider, ILogger<DemoDataSeeder> logger)
{
    private readonly Random _random = new(42); // Deterministic seed

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Check if already fully seeded - verify we have exactly 5 production lines and 50 machines
        using (var checkScope = serviceProvider.CreateScope())
        {
            var checkContext = checkScope.ServiceProvider.GetRequiredService<DigitalTwinDbContext>();
            var lineCount = await checkContext.ProductionLines.CountAsync(cancellationToken);
            var machineCount = await checkContext.Machines.CountAsync(cancellationToken);
            
            if (lineCount == 5 && machineCount == 50)
            {
                logger.LogInformation("Database already fully seeded with {LineCount} production lines and {MachineCount} machines.", lineCount, machineCount);
                return;
            }
            
            if (lineCount > 0 || machineCount > 0)
            {
                logger.LogWarning("Database has partial data ({LineCount} lines, {MachineCount} machines). Clearing before reseeding...", lineCount, machineCount);
                // Clear existing data to start fresh
                await checkContext.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""Machines"", ""ProductionLines"", ""TelemetryData"", ""Predictions"", ""Alerts"", ""MaintenanceRecords"" CASCADE");
            }
        }

        const int maxRetries = 3;
        var retryDelay = TimeSpan.FromSeconds(2);
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                logger.LogInformation("Starting deterministic demo data seeding (attempt {Attempt}/{MaxAttempts})...", attempt, maxRetries);

                await SeedProductionLinesAndMachinesAsync(cancellationToken);
                await SeedTelemetryDataAsync(cancellationToken);
                await SeedPredictionsAsync(cancellationToken);
                await SeedAlertsAsync(cancellationToken);
                await SeedMaintenanceRecordsAsync(cancellationToken);

                logger.LogInformation("Demo data seeding completed successfully.");
                return; // Success, exit retry loop
            }
            catch (DbUpdateConcurrencyException ex) when (attempt < maxRetries)
            {
                logger.LogWarning(ex, "Concurrency error during seeding (attempt {Attempt}/{MaxAttempts}). Retrying in {Delay}s...", 
                    attempt, maxRetries, retryDelay.TotalSeconds);
                await Task.Delay(retryDelay, cancellationToken);
                retryDelay = TimeSpan.FromSeconds(retryDelay.TotalSeconds * 2); // Exponential backoff
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding demo data (attempt {Attempt}/{MaxAttempts}).", attempt, maxRetries);
                if (attempt == maxRetries)
                    throw;
                
                logger.LogWarning("Retrying in {Delay}s...", retryDelay.TotalSeconds);
                await Task.Delay(retryDelay, cancellationToken);
                retryDelay = TimeSpan.FromSeconds(retryDelay.TotalSeconds * 2);
            }
        }
    }

    private async Task SeedProductionLinesAndMachinesAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DigitalTwinDbContext>();
        
        // Check if already fully seeded
        var existingLines = await context.ProductionLines.CountAsync(cancellationToken);
        var existingMachines = await context.Machines.CountAsync(cancellationToken);
        
        if (existingLines >= 5 && existingMachines >= 50)
        {
            logger.LogInformation("Production lines ({LineCount}) and machines ({MachineCount}) already seeded. Skipping.", existingLines, existingMachines);
            return;
        }
        
        logger.LogInformation("Seeding 5 Production Lines and 50 Machines using raw SQL...");

        var machineTypes = new[] { "CNC", "Conveyor", "Press", "Welder", "Robot" };
        var lineIds = new List<Guid>();
        var now = DateTime.UtcNow;
        
        // Insert production lines using raw SQL to bypass auditing
        for (int i = 1; i <= 5; i++)
        {
            var lineId = Guid.NewGuid();
            lineIds.Add(lineId);
            var lineName = $"Production Line {i:00} - {machineTypes[i - 1]}s";
            
            var sql = "INSERT INTO \"ProductionLines\" (\"Id\", \"Name\", \"Configuration\") VALUES ({0}, {1}, {2}::jsonb)";
            var parameters = new object[] { lineId, lineName, "{}" };
            
            logger.LogInformation("Inserting ProductionLine {Count}/5: {Name}", i, lineName);
            await context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
        }

        logger.LogInformation("Successfully inserted {Count} production lines", lineIds.Count);

        // Insert machines using raw SQL
        var statuses = new[] { "Operational", "Warning", "Critical", "Maintenance", "Offline" };
        var healthStatuses = new[] { "Healthy", "Normal", "MinorDegradation", "SignificantDegradation", "FailureImminent" };
        
        int machineCount = 0;
        for (int i = 0; i < 5; i++)
        {
            for (int j = 1; j <= 10; j++)
            {
                machineCount++;
                var machineId = Guid.NewGuid();
                var lineId = lineIds[i];
                var machineName = $"{machineTypes[i]} Unit {j:00}";
                var machineType = machineTypes[i];
                var status = statuses[_random.Next(statuses.Length)];
                var healthStatus = healthStatuses[_random.Next(healthStatuses.Length)];
                var location = $"Station {j:00}";
                var failureProb = _random.NextDouble() * 0.9;
                var rul = _random.Next(1, 400);
                var createdAt = now.AddDays(-_random.Next(100, 1000));
                var lastMaintenance = now.AddDays(-_random.Next(5, 90));
                
                var serialNumber = $"SN-{machineTypes[i]}-{j:0000}-{_random.Next(1000, 9999)}";
                var manufacturer = $"Manufacturer {_random.Next(1, 10)}";
                var model = $"Model-{_random.Next(100, 999)}";
                
                var sql = @"INSERT INTO ""Machines"" (
                    ""Id"", ""Name"", ""Type"", ""ProductionLineId"", ""Location"", 
                    ""Status"", ""IsActive"", ""RemainingUsefulLifeDays"", ""FailureProbability"", 
                    ""HealthStatus"", ""InstallationDate"", ""LastMaintenanceDate"", 
                    ""CreatedAt"", ""UpdatedAt"", ""Configuration"", ""Properties"",
                    ""SerialNumber"", ""Manufacturer"", ""Model""
                ) VALUES (
                    {0}, {1}, {2}, {3}, {4},
                    {5}, {6}, {7}, {8},
                    {9}, {10}, {11},
                    {12}, {13}, {14}::jsonb, {15}::jsonb,
                    {16}, {17}, {18}
                )";
                
                var parameters = new object[] {
                    machineId, machineName, machineType, lineId, location,
                    status, true, rul, failureProb,
                    healthStatus, createdAt, lastMaintenance,
                    now, now, "{}", "{}",
                    serialNumber, manufacturer, model
                };
                
                try
                {
                    await context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
                    
                    if (machineCount % 10 == 0)
                    {
                        logger.LogInformation("Inserted {Count}/50 machines...", machineCount);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to insert machine {Count}: {Name}", machineCount, machineName);
                    throw;
                }
            }
        }
        
        logger.LogInformation("Successfully inserted all {Count} machines", machineCount);
    }

    private async Task SeedTelemetryDataAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DigitalTwinDbContext>();
        
        logger.LogInformation("Seeding 30 days of Telemetry Data for all 50 machines...");

        var machines = await context.Machines.Select(m => m.Id).ToListAsync(cancellationToken);
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-30);
        
        int telemetryCount = 0;
        int totalTelemetry = machines.Count * (30 * 6); // 30 days, 6 points per day (every 4 hours)
        
        foreach (var machineId in machines)
        {
            // Seed 1 point every 4 hours
            for (var current = startDate; current <= endDate; current = current.AddHours(4))
            {
                var healthScore = 100 - (_random.NextDouble() * 20);
                var telemetryId = Guid.NewGuid();
                var temperature = 45 + (_random.NextDouble() * 30);
                var vibration = 0.5 + (_random.NextDouble() * 2.5);
                var pressure = 101.3 + (_random.NextDouble() * 10);
                var rpm = 1500 + _random.Next(-50, 50);
                
                var sql = @"INSERT INTO ""TelemetryData"" (
                    ""Id"", ""MachineId"", ""Timestamp"", ""DataType"", 
                    ""Temperature"", ""Vibration"", ""Pressure"", ""Rpm"", ""HealthScore"", ""Data""
                ) VALUES (
                    {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}::jsonb
                )";
                
                var parameters = new object[] {
                    telemetryId, machineId, current, "AggregateList",
                    temperature, vibration, pressure, rpm, healthScore, "{}"
                };
                
                await context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
                
                telemetryCount++;
                
                if (telemetryCount % 1000 == 0)
                {
                    logger.LogInformation("Inserted {Count}/{Total} telemetry records...", telemetryCount, totalTelemetry);
                    // Small delay to prevent overwhelming the database
                    await Task.Delay(10, cancellationToken);
                }
            }
        }
        
        logger.LogInformation("Successfully inserted {Count} telemetry records", telemetryCount);
    }

    private async Task SeedPredictionsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DigitalTwinDbContext>();
        
        // Check if already seeded
        var existingCount = await context.Predictions.CountAsync(cancellationToken);
        if (existingCount >= 50)
        {
            logger.LogInformation("Predictions already seeded ({Count} existing). Skipping.", existingCount);
            return;
        }
        
        logger.LogInformation("Seeding Predictions...");
        var machines = await context.Machines.Select(m => new { m.Id, m.RemainingUsefulLifeDays, m.FailureProbability, m.HealthStatus }).ToListAsync(cancellationToken);
        
        int count = 0;
        foreach (var machine in machines)
        {
            var baseRul = machine.RemainingUsefulLifeDays ?? 100.0;
            var failureProb = machine.FailureProbability ?? 0.1;
            var healthStatus = machine.HealthStatus ?? HealthClassification.Normal;
            var conf = 0.85 + (_random.NextDouble() * 0.1);
            var lowerBound = baseRul * (1 - (1 - conf) * 0.5);
            var upperBound = baseRul * (1 + (1 - conf) * 0.5);

            var sql = @"INSERT INTO ""Predictions"" (
                ""Id"", ""MachineId"", ""RemainingUsefulLifeDays"", ""Confidence"", ""FailureProbability"", 
                ""HealthStatus"", ""ModelVersion"", ""RulLowerBound"", ""RulUpperBound"", ""CreatedAt"", ""UpdatedAt"", ""PredictionTime""
            ) VALUES (
                {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}
            )";
            
            var parameters = new object[] {
                Guid.NewGuid(), machine.Id, baseRul, conf, failureProb,
                healthStatus.ToString(), "v1.2.0-rf", lowerBound, upperBound, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow
            };
            
            await context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
            count++;
        }
        
        logger.LogInformation("Successfully inserted {Count} predictions", count);
    }

    private async Task SeedAlertsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DigitalTwinDbContext>();
        
        // Check if already seeded (50 machines × 3 alerts = 150)
        var existingCount = await context.Alerts.CountAsync(cancellationToken);
        if (existingCount >= 150)
        {
            logger.LogInformation("Alerts already seeded ({Count} existing). Skipping.", existingCount);
            return;
        }
        
        logger.LogInformation("Seeding Alerts...");
        var machines = await context.Machines.Select(m => m.Id).ToListAsync(cancellationToken);
        var severities = new[] { "Warning", "Critical", "Info" };
        var categories = new[] { "Temperature", "Vibration", "RUL Threshold", "Maintenance Required" };
        
        int count = 0;
        foreach (var machineId in machines)
        {
            for (int i = 0; i < 3; i++)
            {
                var isResolved = i < 2;
                var severity = severities[_random.Next(severities.Length)];
                var category = categories[_random.Next(categories.Length)];
                var message = $"{severity} Anomaly Detected: Sensor readings crossed the established threshold.";
                var recommendation = "Inspect machine parts for excessive wear.";
                var createdAt = DateTime.UtcNow.AddDays(-_random.Next(1, 30));
                var acknowledgedAt = isResolved ? createdAt.AddHours(2) : (DateTime?)null;
                var resolvedAt = isResolved ? createdAt.AddHours(5) : (DateTime?)null;
                var acknowledgedBy = isResolved ? "demo.operator@example.com" : null;
                var isAcknowledged = isResolved;

                var sql = @"INSERT INTO ""Alerts"" (
                    ""Id"", ""MachineId"", ""Message"", ""Title"", ""Description"", ""Severity"", ""Status"",
                    ""Category"", ""RecommendedAction"", ""IsAcknowledged"", ""CreatedAt"", ""AcknowledgedAt"", ""ResolvedAt"", ""AcknowledgedBy""
                ) VALUES (
                    {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}
                )";
                
                var title = $"{severity} Alert";
                var description = message;
                var status = isResolved ? "resolved" : (isAcknowledged ? "acknowledged" : "active");
                
                var parameters = new object[] {
                    Guid.NewGuid(), machineId, message, title, description, severity, status,
                    category, recommendation, isAcknowledged, createdAt, acknowledgedAt, resolvedAt, acknowledgedBy
                };
                
                await context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
                count++;
            }
        }
        
        logger.LogInformation("Successfully inserted {Count} alerts", count);
    }

    private async Task SeedMaintenanceRecordsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DigitalTwinDbContext>();
        
        // Check if already seeded (50 machines × 2 records = 100)
        var existingCount = await context.MaintenanceRecords.CountAsync(cancellationToken);
        if (existingCount >= 100)
        {
            logger.LogInformation("Maintenance records already seeded ({Count} existing). Skipping.", existingCount);
            return;
        }
        
        logger.LogInformation("Seeding Maintenance Records...");
        var machines = await context.Machines.Select(m => m.Id).ToListAsync(cancellationToken);
        var types = new[] { "Preventive", "Corrective", "Predictive" };
        
        int count = 0;
        foreach (var machineId in machines)
        {
            for (int i = 0; i < 2; i++)
            {
                var isCompleted = i == 0;
                var type = types[_random.Next(types.Length)];
                var plannedDate = isCompleted ? DateTime.UtcNow.AddDays(-_random.Next(5, 60)) : DateTime.UtcNow.AddDays(_random.Next(2, 14));
                var description = isCompleted ? "Routine oil change and recalibration." : "Upcoming predictive bearing replacement.";
                var technician = "John Smith (Demo)";
                var completionDate = isCompleted ? plannedDate.AddHours(4) : (DateTime?)null;
                var cost = isCompleted ? (decimal)(_random.NextDouble() * 5000) : (decimal?)null;
                var status = isCompleted ? "Completed" : "Scheduled";

                var sql = @"INSERT INTO ""MaintenanceRecords"" (
                    ""Id"", ""MachineId"", ""Type"", ""Description"", ""Date"", ""PlannedDate"",
                    ""Technician"", ""Status"", ""CompletionDate"", ""Cost"", ""CostCurrency"", ""Notes"", ""PartsReplaced""
                ) VALUES (
                    {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}::jsonb
                )";
                
                var notes = isCompleted ? "Maintenance completed successfully." : "Scheduled maintenance pending.";
                var costCurrency = "USD";
                var partsReplaced = isCompleted ? "[\"Bearings\", \"Filter\"]" : "[]";
                
                var parameters = new object[] {
                    Guid.NewGuid(), machineId, type, description, plannedDate, plannedDate,
                    technician, status, completionDate, cost ?? 0m, costCurrency, notes, partsReplaced
                };
                
                await context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
                count++;
            }
        }
        
        logger.LogInformation("Successfully inserted {Count} maintenance records", count);
    }
}
