using System.Text.Json;
using DigitalTwinPlatform.Domain.Common;
using DigitalTwinPlatform.Domain.Entities.Enums;
using DigitalTwinPlatform.Domain.Enums;
using DigitalTwinPlatform.Domain.ValueObjects;

namespace DigitalTwinPlatform.Domain.Entities;

public class Machine
{
    private MachineName _name = null!;
    private MachineType _type = null!;
    private EquipmentStatus _status = EquipmentStatus.Operational;
    private bool _isActive = true;

    //  Private constructor for EF Core
    private Machine() { }

    //  Factory method with validation
    public static Result<Machine> Create(
        MachineName name,
        MachineType type,
        Guid? productionLineId = null,
        Guid? tenantId = null)
    {
        var machine = new Machine
        {
            Id = Guid.NewGuid(),
            _name = name,
            _type = type,
            ProductionLineId = productionLineId,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return new Result<Machine>.Success(machine);
    }

    //  Properties (read-only)
    public Guid Id { get; private set; }
    public MachineName Name => _name;
    public MachineType Type => _type;
    public Guid? TenantId { get; private set; }
    public bool IsActive => _isActive;
    public EquipmentStatus Status => _status;
    
    // Enhanced configuration fields
    public string Location { get; private set; } = string.Empty;
    public DateTime? InstallationDate { get; private set; }
    public DateTime? LastMaintenanceDate { get; private set; }
    public DateTime? NextMaintenanceDate { get; private set; }
    
    public JsonDocument Configuration { get; private set; } = JsonDocument.Parse("{}");
    public JsonDocument Properties { get; private set; } = JsonDocument.Parse("{}");
    
    public double? RemainingUsefulLifeDays { get; private set; }
    public double? FailureProbability { get; private set; }
    public HealthClassification? HealthStatus { get; private set; }
    
    public ICollection<MaintenanceRecord> MaintenanceRecords { get; private set; } = [];
    public ICollection<TelemetryData> Telemetry { get; private set; } = [];
    public ICollection<Prediction> Predictions { get; private set; } = [];
    public Guid? ProductionLineId { get; private set; }
    public ProductionLine? ProductionLine { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    //  Domain methods (invariants enforced)
    public Result Deactivate()
    {
        if (!_isActive)
            return new Result.Failure("Machine is already inactive");

        _isActive = false;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result Activate()
    {
        if (_isActive)
            return new Result.Failure("Machine is already active");

        _isActive = true;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdateStatus(EquipmentStatus newStatus)
    {
        if (newStatus == _status)
            return new Result.Failure($"Machine status is already {newStatus}");

        _status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdateHealthMetrics(
        double? remainingUsefulLifeDays,
        double? failureProbability,
        HealthClassification? healthStatus)
    {
        if (remainingUsefulLifeDays is not null && remainingUsefulLifeDays < 0)
            return new Result.Failure("Remaining useful life cannot be negative");

        if (failureProbability is not null && (failureProbability < 0 || failureProbability > 1))
            return new Result.Failure("Failure probability must be between 0 and 1");

        RemainingUsefulLifeDays = remainingUsefulLifeDays;
        FailureProbability = failureProbability;
        HealthStatus = healthStatus;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdateProperties(JsonDocument properties)
    {
        if (properties == null)
            return new Result.Failure("Properties cannot be null");

        Properties = properties;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdateConfiguration(JsonDocument configuration)
    {
        if (configuration == null)
            return new Result.Failure("Configuration cannot be null");

        Configuration = configuration;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdateLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
            return new Result.Failure("Location cannot be empty");

        Location = location;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdateInstallationDate(DateTime installationDate)
    {
        if (installationDate > DateTime.UtcNow)
            return new Result.Failure("Installation date cannot be in the future");

        InstallationDate = installationDate;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result RecordMaintenance(DateTime maintenanceDate)
    {
        if (maintenanceDate > DateTime.UtcNow)
            return new Result.Failure("Maintenance date cannot be in the future");

        LastMaintenanceDate = maintenanceDate;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result ScheduleNextMaintenance(DateTime nextMaintenanceDate)
    {
        if (nextMaintenanceDate <= DateTime.UtcNow)
            return new Result.Failure("Next maintenance date must be in the future");

        NextMaintenanceDate = nextMaintenanceDate;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }
}
