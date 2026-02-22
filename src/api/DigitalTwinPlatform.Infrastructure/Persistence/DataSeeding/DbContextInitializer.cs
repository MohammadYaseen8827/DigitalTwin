using Microsoft.EntityFrameworkCore;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.ValueObjects;
using DigitalTwinPlatform.Domain.Entities.Simulation;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Infrastructure.Persistence.DataSeeding;

/// <summary>
/// Initializes the database with seed data on first run.
/// Only executes if no data exists in the database.
/// </summary>
public class DbContextInitializer
{
    private readonly DigitalTwinDbContext _context;
    private readonly ILogger<DbContextInitializer> _logger;

    public DbContextInitializer(DigitalTwinDbContext context, ILogger<DbContextInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Initializes database with seed data if empty.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            // Apply pending migrations
            await _context.Database.MigrateAsync();

            // Only seed if database is empty
            if (await _context.Machines.AnyAsync())
            {
                _logger.LogInformation("Database already populated. Skipping seed data initialization.");
                return;
            }

            _logger.LogInformation("Starting database seed data initialization...");

            // Seed data in order of dependencies
            await SeedProductionLinesAsync();
            await SeedMachinesAsync();
            await SeedAlertsAsync();
            await SeedSimulationsAsync();

            _logger.LogInformation("Database seed data initialization completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedMachinesAsync()
    {
        var machines = new List<Machine>();
            
        // Create machines using the factory method with proper Result handling
        var cncMachineNameResult = MachineName.Create("CNC Machine #1");
        var cncMachineTypeResult = MachineType.Create("CNC Machine");
        var cncMachineResult = cncMachineNameResult.Match<Machine?>(
            onSuccess: name => cncMachineTypeResult.Match<Machine?>(
                onSuccess: type => Machine.Create(name, type).Match<Machine?>(
                    onSuccess: machine =>
                    {
                        machine.UpdateLocation("Manufacturing Floor - Zone A");
                        machine.UpdateBasicInfo("CNC-001-2024", "FANUC", "Robodrill", 5);
                        machine.UpdateInstallationDate(DateTime.UtcNow.AddYears(-3));
                        machine.UpdateWarrantyExpiry(DateTime.UtcNow.AddYears(1));
                        machine.RecordMaintenance(DateTime.UtcNow.AddDays(-15));
                        machine.ScheduleNextMaintenance(DateTime.UtcNow.AddDays(30));
                        machine.UpdateMaintenanceInterval(30);
                        return machine;
                    },
                    onFailure: error =>
                    {
                        _logger.LogError("Failed to create CNC Machine #1: {Error}", error);
                        return null;
                    }),
                onFailure: error =>
                {
                    _logger.LogError("Failed to create CNC Machine type: {Error}", error);
                    return null;
                }),
            onFailure: error =>
            {
                _logger.LogError("Failed to create CNC Machine name: {Error}", error);
                return null;
            });
            
        if (cncMachineResult != null)
        {
            machines.Add(cncMachineResult);
        }
    
        var injectionMolderNameResult = MachineName.Create("Injection Molder #1");
        var injectionMolderTypeResult = MachineType.Create("Injection Molder");
        var injectionMolderResult = injectionMolderNameResult.Match<Machine?>(
            onSuccess: name => injectionMolderTypeResult.Match<Machine?>(
                onSuccess: type => Machine.Create(name, type).Match<Machine?>(
                    onSuccess: machine =>
                    {
                        machine.UpdateLocation("Manufacturing Floor - Zone B");
                        machine.UpdateBasicInfo("INJ-001-2023", "Engel", "Victory 500", 4);
                        machine.UpdateInstallationDate(DateTime.UtcNow.AddYears(-2));
                        machine.UpdateWarrantyExpiry(DateTime.UtcNow.AddYears(2));
                        machine.RecordMaintenance(DateTime.UtcNow.AddDays(-20));
                        machine.ScheduleNextMaintenance(DateTime.UtcNow.AddDays(40));
                        machine.UpdateMaintenanceInterval(45);
                        return machine;
                    },
                    onFailure: error =>
                    {
                        _logger.LogError("Failed to create Injection Molder #1: {Error}", error);
                        return null;
                    }),
                onFailure: error =>
                {
                    _logger.LogError("Failed to create Injection Molder type: {Error}", error);
                    return null;
                }),
            onFailure: error =>
            {
                _logger.LogError("Failed to create Injection Molder name: {Error}", error);
                return null;
            });
            
        if (injectionMolderResult != null)
        {
            machines.Add(injectionMolderResult);
        }
    
        var pressMachineNameResult = MachineName.Create("Press Machine #1");
        var pressMachineTypeResult = MachineType.Create("Hydraulic Press");
        var pressMachineResult = pressMachineNameResult.Match<Machine?>(
            onSuccess: name => pressMachineTypeResult.Match<Machine?>(
                onSuccess: type => Machine.Create(name, type).Match<Machine?>(
                    onSuccess: machine =>
                    {
                        machine.UpdateLocation("Manufacturing Floor - Zone C");
                        machine.UpdateBasicInfo("PRESS-001-2023", "Schuler", "EconoPress", 3);
                        machine.UpdateInstallationDate(DateTime.UtcNow.AddYears(-4));
                        machine.UpdateWarrantyExpiry(DateTime.UtcNow.AddMonths(6));
                        machine.RecordMaintenance(DateTime.UtcNow.AddDays(-10));
                        machine.ScheduleNextMaintenance(DateTime.UtcNow.AddDays(25));
                        machine.UpdateMaintenanceInterval(25);
                        return machine;
                    },
                    onFailure: error =>
                    {
                        _logger.LogError("Failed to create Press Machine #1: {Error}", error);
                        return null;
                    }),
                onFailure: error =>
                {
                    _logger.LogError("Failed to create Press Machine type: {Error}", error);
                    return null;
                }),
            onFailure: error =>
            {
                _logger.LogError("Failed to create Press Machine name: {Error}", error);
                return null;
            });
            
        if (pressMachineResult != null)
        {
            machines.Add(pressMachineResult);
        }
    
        var robotArmNameResult = MachineName.Create("Robot Arm #1");
        var robotArmTypeResult = MachineType.Create("Robot");
        var robotArmResult = robotArmNameResult.Match<Machine?>(
            onSuccess: name => robotArmTypeResult.Match<Machine?>(
                onSuccess: type => Machine.Create(name, type).Match<Machine?>(
                    onSuccess: machine =>
                    {
                        machine.UpdateLocation("Assembly Line - Zone A");
                        machine.UpdateBasicInfo("ROBOT-001-2024", "ABB", "IRB 6700", 5);
                        machine.UpdateInstallationDate(DateTime.UtcNow.AddYears(-1));
                        machine.UpdateWarrantyExpiry(DateTime.UtcNow.AddYears(3));
                        machine.RecordMaintenance(DateTime.UtcNow.AddDays(-5));
                        machine.ScheduleNextMaintenance(DateTime.UtcNow.AddDays(50));
                        machine.UpdateMaintenanceInterval(60);
                        return machine;
                    },
                    onFailure: error =>
                    {
                        _logger.LogError("Failed to create Robot Arm #1: {Error}", error);
                        return null;
                    }),
                onFailure: error =>
                {
                    _logger.LogError("Failed to create Robot Arm type: {Error}", error);
                    return null;
                }),
            onFailure: error =>
            {
                _logger.LogError("Failed to create Robot Arm name: {Error}", error);
                return null;
            });
            
        if (robotArmResult != null)
        {
            machines.Add(robotArmResult);
        }
    
        var conveyorNameResult = MachineName.Create("Conveyor System #1");
        var conveyorTypeResult = MachineType.Create("Conveyor");
        var conveyorResult = conveyorNameResult.Match<Machine?>(
            onSuccess: name => conveyorTypeResult.Match<Machine?>(
                onSuccess: type => Machine.Create(name, type).Match<Machine?>(
                    onSuccess: machine =>
                    {
                        machine.UpdateLocation("Logistics - Zone A");
                        machine.UpdateBasicInfo("CONV-001-2022", "Siemens", "Sitrans", 2);
                        machine.UpdateInstallationDate(DateTime.UtcNow.AddYears(-5));
                        machine.UpdateWarrantyExpiry(DateTime.UtcNow.AddMonths(-3));
                        machine.RecordMaintenance(DateTime.UtcNow.AddDays(-30));
                        machine.ScheduleNextMaintenance(DateTime.UtcNow.AddDays(10));
                        machine.UpdateMaintenanceInterval(35);
                        // Update status to warning
                        machine.UpdateStatus(Domain.Enums.EquipmentStatus.Warning);
                        return machine;
                    },
                    onFailure: error =>
                    {
                        _logger.LogError("Failed to create Conveyor System #1: {Error}", error);
                        return null;
                    }),
                onFailure: error =>
                {
                    _logger.LogError("Failed to create Conveyor System type: {Error}", error);
                    return null;
                }),
            onFailure: error =>
            {
                _logger.LogError("Failed to create Conveyor System name: {Error}", error);
                return null;
            });
            
        if (conveyorResult != null)
        {
            machines.Add(conveyorResult);
        }
    
        // Check if machines already exist to avoid duplicates
        foreach (var machine in machines)
        {
            var existing = await _context.Machines.FirstOrDefaultAsync(m => m.Name.Value == machine.Name.Value);
            if (existing == null)
            {
                await _context.Machines.AddAsync(machine);
            }
        }
            
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} machines.", machines.Count);
    }

    private async Task SeedProductionLinesAsync()
    {
        var productionLines = new List<ProductionLine>
        {
            new ProductionLine
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Main Production Line",
            },
            new ProductionLine
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name = "Secondary Assembly Line",
            },
            new ProductionLine
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Name = "Quality Control Line",
            }
        };

        // Check if production lines already exist to avoid duplicates
        foreach (var productionLine in productionLines)
        {
            var existing = await _context.ProductionLines.FirstOrDefaultAsync(pl => pl.Id == productionLine.Id);
            if (existing == null)
            {
                await _context.ProductionLines.AddAsync(productionLine);
            }
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} production lines.", productionLines.Count);
    }

    private async Task SeedAlertsAsync()
    {
        // We'll seed alerts after machines are saved so we can reference their actual IDs
        var now = DateTime.UtcNow;
        
        // Get the machines to reference their actual IDs
        var cncMachine = await _context.Machines.FirstOrDefaultAsync(m => m.Name.Value == "CNC Machine #1");
        var injectionMolder = await _context.Machines.FirstOrDefaultAsync(m => m.Name.Value == "Injection Molder #1");
        var conveyor = await _context.Machines.FirstOrDefaultAsync(m => m.Name.Value == "Conveyor System #1");
        
        var alerts = new List<Alert>();
        
        if (conveyor != null)
        {
            // Create alerts with proper handling of the Alert factory method
            var alert1 = new Alert
            {
                Id = Guid.NewGuid(),
                MachineId = conveyor.Id, // Conveyor System #1 (Warning status)
                Message = "Conveyor system showing elevated temperature",
                Title = "Conveyor System Temperature",
                Description = "Conveyor system showing elevated temperature",
                Severity = AlertSeverity.Warning,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };
            alerts.Add(alert1);
            
            var alert2 = new Alert
            {
                Id = Guid.NewGuid(),
                MachineId = conveyor.Id, // Conveyor System #1
                Message = "Maintenance due in 10 days",
                Title = "Maintenance Due",
                Description = "Maintenance due in 10 days",
                Severity = AlertSeverity.Warning,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };
            alerts.Add(alert2);
        }
        
        if (cncMachine != null)
        {
            var alert3 = new Alert
            {
                Id = Guid.NewGuid(),
                MachineId = cncMachine.Id, // CNC Machine #1
                Message = "Routine maintenance completed successfully",
                Title = "Maintenance Completed",
                Description = "Routine maintenance completed successfully",
                Severity = AlertSeverity.Info,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };
            alert3.Resolve(); // Mark as resolved
            alerts.Add(alert3);
        }
        
        if (injectionMolder != null)
        {
            var alert4 = new Alert
            {
                Id = Guid.NewGuid(),
                MachineId = injectionMolder.Id, // Injection Molder #1
                Message = "Vibration levels within normal range",
                Title = "Vibration Status Normal",
                Description = "Vibration levels within normal range",
                Severity = AlertSeverity.Info,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };
            alert4.Resolve(); // Mark as resolved
            alerts.Add(alert4);
        }
        
        // Check if alerts already exist to avoid duplicates
        foreach (var alert in alerts)
        {
            var existing = await _context.Alerts.FirstOrDefaultAsync(a => a.Message == alert.Message && a.MachineId == alert.MachineId);
            if (existing == null)
            {
                await _context.Alerts.AddAsync(alert);
            }
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} alerts.", alerts.Count);
    }

    private async Task SeedSimulationsAsync()
    {
        // Get the machines to reference their actual IDs
        var cncMachine = await _context.Machines.FirstOrDefaultAsync(m => m.Name.Value == "CNC Machine #1");
        var injectionMolder = await _context.Machines.FirstOrDefaultAsync(m => m.Name.Value == "Injection Molder #1");
        
        var simulations = new List<SimulationState>();
        
        if (cncMachine != null)
        {
            simulations.Add(new SimulationState
            {
                Id = Guid.NewGuid(),
                MachineId = cncMachine.Id, // CNC Machine #1
                Status = SimulationStatus.Completed,
                TotalSteps = 100,
                CurrentStep = 100,
                StartTime = DateTime.UtcNow.AddDays(-7),
                EndTime = DateTime.UtcNow.AddDays(-1),
                Parameters = new Dictionary<string, object> { { "degradationModel", "linear" }, { "predictionType", "rul" } },
                Metrics = new Dictionary<string, object> { { "predictedRUL", Random.Shared.NextDouble() * 500 + 100 } }
            });
        }
        
        if (injectionMolder != null)
        {
            simulations.Add(new SimulationState
            {
                Id = Guid.NewGuid(),
                MachineId = injectionMolder.Id, // Injection Molder #1
                Status = SimulationStatus.Completed,
                TotalSteps = 100,
                CurrentStep = 100,
                StartTime = DateTime.UtcNow.AddDays(-5),
                EndTime = DateTime.UtcNow.AddDays(-2),
                Parameters = new Dictionary<string, object> { { "degradationModel", "exponential" }, { "predictionType", "rul" } },
                Metrics = new Dictionary<string, object> { { "predictedRUL", Random.Shared.NextDouble() * 500 + 100 } }
            });
        }

        // Check if simulations already exist to avoid duplicates
        foreach (var simulation in simulations)
        {
            var existing = await _context.SimulationStates.FirstOrDefaultAsync(s => s.Id == simulation.Id);
            if (existing == null)
            {
                await _context.SimulationStates.AddAsync(simulation);
            }
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} simulations.", simulations.Count);
    }
}
