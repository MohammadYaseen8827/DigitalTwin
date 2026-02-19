using System.Text.Json;
using DigitalTwinPlatform.Domain.Common;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Entities.Auth;
using DigitalTwinPlatform.Domain.Entities.Simulation;
using DigitalTwinPlatform.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DigitalTwinPlatform.Infrastructure.Persistence;

public class DigitalTwinDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DigitalTwinDbContext(
        DbContextOptions<DigitalTwinDbContext> options,
        IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<ProductionLine> ProductionLines => Set<ProductionLine>();
    public DbSet<TelemetryData> TelemetryData => Set<TelemetryData>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();
    public DbSet<Prediction> Predictions => Set<Prediction>();
    public DbSet<SimulationState> SimulationStates => Set<SimulationState>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ModelVersion> ModelVersions => Set<ModelVersion>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChanges(auditEntries);
        return result;
    }

    private List<AuditLog> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditLog>();
        var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditLog = new AuditLog
            {
                EntityName = entry.Entity.GetType().Name,
                EntityId = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString() ?? "unknown",
                Action = entry.State.ToString(),
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Changes = entry.State == EntityState.Modified 
                    ? System.Text.Json.JsonSerializer.Serialize(entry.Properties
                        .Where(p => p.IsModified)
                        .ToDictionary(p => p.Metadata.Name, p => new { Old = p.OriginalValue, New = p.CurrentValue }))
                    : string.Empty
            };
            auditEntries.Add(auditLog);
        }

        return auditEntries;
    }

    private async Task OnAfterSaveChanges(List<AuditLog> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0) return;

        AuditLogs.AddRange(auditEntries);
        await base.SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("uuid-ossp");

        // Value Converters
        var machineNameConverter = new ValueConverter<MachineName, string>(
            v => v.Value,
            v => new MachineName(v));
        
        var machineTypeConverter = new ValueConverter<MachineType, string>(
            v => v.Value,
            v => new MachineType(v));

        modelBuilder.Entity<Machine>(entity =>
        {
            entity.HasKey(x => x.Id);
            
            entity.Property(x => x.Location)
                .HasMaxLength(200)
                .HasDefaultValue(string.Empty);
            
            entity.Property(x => x.InstallationDate)
                .HasColumnType("timestamp with time zone");
            
            entity.Property(x => x.LastMaintenanceDate)
                .HasColumnType("timestamp with time zone");
            
            entity.Property("_name")
                .HasColumnName("Name")
                .HasConversion(machineNameConverter)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property("_type")
                .HasColumnName("Type")
                .HasConversion(machineTypeConverter)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(x => x.IsActive).HasColumnName("IsActive");
            entity.Property(x => x.Status).HasConversion<string>();
            entity.Property(x => x.Configuration)
                .HasColumnType("jsonb")
                .HasDefaultValue(JsonDocument.Parse("{}"));
            entity.Property(x => x.Properties)
                .HasColumnType("jsonb")
                .HasDefaultValue(JsonDocument.Parse("{}"));
            entity.Property(x => x.RemainingUsefulLifeDays);
            entity.Property(x => x.FailureProbability);
            entity.Property(x => x.HealthStatus).HasConversion<string>();
            entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
            
            entity.HasOne(x => x.ProductionLine)
                .WithMany(p => p.Machines)
                .HasForeignKey(x => x.ProductionLineId);
            entity.HasMany(x => x.MaintenanceRecords)
                .WithOne(m => m.Machine)
                .HasForeignKey(m => m.MachineId);
            entity.HasMany(x => x.Telemetry)
                .WithOne(t => t.Machine)
                .HasForeignKey(t => t.MachineId);
            entity.HasMany(x => x.Predictions)
                .WithOne(p => p.Machine)
                .HasForeignKey(p => p.MachineId);
            
            // Indexes for performance
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.IsActive);
            entity.HasIndex(x => x.Location);
            entity.HasIndex(x => x.HealthStatus);
            entity.HasIndex(x => x.RemainingUsefulLifeDays);
        });

        modelBuilder.Entity<ProductionLine>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Configuration)
                .HasColumnType("jsonb")
                .HasDefaultValue(JsonDocument.Parse("{}"));
        });

        modelBuilder.Entity<TelemetryData>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DataType).HasMaxLength(50);
            entity.Property(x => x.Data)
                .HasColumnType("jsonb")
                .HasDefaultValue(JsonDocument.Parse("{}"));
            entity.Property(x => x.Timestamp).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(x => x.Temperature);
            entity.Property(x => x.Vibration);
            entity.Property(x => x.Pressure);
            entity.Property(x => x.Rpm);
            entity.Property(x => x.HealthScore);
            
            entity.HasOne(x => x.Machine)
                .WithMany(m => m.Telemetry)
                .HasForeignKey(x => x.MachineId);
            
            // Indexes for performance
            entity.HasIndex(x => x.MachineId);
            entity.HasIndex(x => x.Timestamp);
            entity.HasIndex(x => new { x.MachineId, x.Timestamp });
            entity.HasIndex(x => x.HealthScore);
        });

        modelBuilder.Entity<MaintenanceRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Date).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(x => x.PlannedDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CompletionDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.Technician).HasMaxLength(200);
            entity.Property(x => x.Cost).HasColumnType("decimal(18,2)");
            entity.Property(x => x.CostCurrency).HasMaxLength(10);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.Property(x => x.PartsReplaced)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasMaxLength(1000);
            entity.Property(x => x.WorkOrderDetails)
                .HasColumnType("jsonb");
            
            entity.HasOne(x => x.Machine)
                .WithMany(m => m.MaintenanceRecords)
                .HasForeignKey(x => x.MachineId);
            entity.HasOne(x => x.Alert)
                .WithMany()
                .HasForeignKey(x => x.AlertId);
            
            // Indexes for performance
            entity.HasIndex(x => x.MachineId);
            entity.HasIndex(x => x.Date);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.Type);
            entity.HasIndex(x => x.Technician);
        });

        modelBuilder.Entity<ModelVersion>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ModelType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Version).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ModelPath).HasMaxLength(500).IsRequired();
            entity.Property(x => x.TrainedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(x => x.Metrics).HasColumnType("jsonb");
            entity.Property(x => x.TrainingDatasetHash).HasMaxLength(64);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
            
            // Indexes
            entity.HasIndex(x => new { x.ModelType, x.Version }).IsUnique();
            entity.HasIndex(x => new { x.ModelType, x.Status });
        });

        modelBuilder.Entity<Prediction>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(x => x.PredictionTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.HealthStatus).HasConversion<string>();
            entity.Property(x => x.Confidence).HasColumnType("decimal(5,4)");
            entity.Property(x => x.FailureProbability).HasColumnType("decimal(5,4)");
            entity.Property(x => x.RemainingUsefulLifeDays).HasColumnType("decimal(18,4)");
            entity.Property(x => x.RulLowerBound).HasColumnType("decimal(18,4)");
            entity.Property(x => x.RulUpperBound).HasColumnType("decimal(18,4)");
            entity.Property(x => x.FeatureContributions)
                .HasColumnType("jsonb");
            entity.Property(x => x.ContributingFactors)
                .HasColumnType("jsonb");
            entity.Property(x => x.ModelVersion).HasMaxLength(50);
            
            entity.HasOne(x => x.Machine)
                .WithMany(m => m.Predictions)
                .HasForeignKey(x => x.MachineId);
            
            // Indexes for performance
            entity.HasIndex(x => x.MachineId);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.Confidence);
            entity.HasIndex(x => x.HealthStatus);
            entity.HasIndex(x => new { x.MachineId, x.CreatedAt });
            entity.HasIndex(x => new { x.HealthStatus, x.FailureProbability });
        });

        modelBuilder.Entity<SimulationState>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion<string>();
            entity.Property(x => x.Parameters)
                .HasColumnType("jsonb")
                .HasDefaultValue(new Dictionary<string, object>());
            entity.Property(x => x.Metrics)
                .HasColumnType("jsonb")
                .HasDefaultValue(new Dictionary<string, object>());
            entity.HasIndex(x => x.StartTime);
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Message).HasMaxLength(1000);
            entity.Property(x => x.Title).HasMaxLength(200);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.Status).HasMaxLength(20);
            entity.Property(x => x.Category).HasMaxLength(100);
            entity.Property(x => x.RecommendedAction).HasMaxLength(500);
            entity.Property(x => x.SuggestedActions).HasMaxLength(1000);
            entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(x => x.AcknowledgedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ResolvedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AcknowledgedBy).HasMaxLength(200);
            
            entity.HasOne(x => x.Machine)
                .WithMany()
                .HasForeignKey(x => x.MachineId);
            entity.HasOne(x => x.RelatedPrediction)
                .WithMany()
                .HasForeignKey(x => x.RelatedPredictionId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Indexes for performance
            entity.HasIndex(x => x.MachineId);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.Severity);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.IsAcknowledged);
            entity.HasIndex(x => new { x.MachineId, x.IsAcknowledged });
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Timestamp).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(100);
            entity.Property(x => x.EntityId).HasMaxLength(100);
            entity.Property(x => x.UserId).HasMaxLength(100);
            entity.Property(x => x.Action).HasMaxLength(20);
            entity.Property(x => x.Changes).HasColumnType("text");
            
            entity.HasIndex(x => x.Timestamp);
            entity.HasIndex(x => x.EntityName);
            entity.HasIndex(x => x.UserId);
        });
    }
}
