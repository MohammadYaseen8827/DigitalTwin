using Asp.Versioning;
using DigitalTwinPlatform.API.Services.Core;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Entities.Enums;
using DigitalTwinPlatform.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

/// <summary>
/// API controller for dashboard statistics.
/// Provides aggregated metrics for the dashboard display.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[ApiVersion("1.0")]
public class DashboardController(
    IAlertService alertService,
    IMachineRepository machineRepository) : ControllerBase
{
    private readonly IMachineRepository _machineRepository = machineRepository;

    /// <summary>
    /// Gets dashboard statistics for the main view.
    /// </summary>
    /// <returns>Aggregated statistics for machines, alerts, and health.</returns>
    /// <response code="200">Returns dashboard statistics successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        var machines = (await _machineRepository.GetAllAsync()).ToList();
        
        // Calculate statistics based on Machine entity properties
        var runningCount = machines.Count(m => m.Status == EquipmentStatus.Operational);
        var maintenanceCount = machines.Count(m => m.Status == EquipmentStatus.Maintenance);
        var errorCount = machines.Count(m => m.Status == EquipmentStatus.Critical);
        
        // Calculate average health from HealthStatus enum (0-100 scale)
        var healthScores = machines
            .Where(m => m.HealthStatus.HasValue)
            .Select(m => (double)MapHealthStatusToScore(m.HealthStatus.Value))
            .ToList();
        var avgHealth = healthScores.Any() ? Math.Round(healthScores.Average(), 1) : 0;
        
        // Calculate pending maintenance (machines with last maintenance > 30 days ago)
        var pendingMaintenance = machines.Count(m => 
            m.LastMaintenanceDate == null || 
            (DateTime.UtcNow - m.LastMaintenanceDate.Value).TotalDays > 30);

        var stats = new DashboardStatsDto
        {
            TotalMachines = machines.Count(),
            RunningMachines = runningCount,
            MaintenanceMachines = maintenanceCount,
            ErrorMachines = errorCount,
            AverageHealthScore = avgHealth,
            ActiveAlerts = (await alertService.GetActiveAlertsAsync()).Count(),
            PendingMaintenance = pendingMaintenance
        };

        return Ok(stats);
    }

    private static int MapHealthStatusToScore(HealthClassification status)
    {
        return status switch
        {
            HealthClassification.Healthy => 100,
            HealthClassification.Normal => 85,
            HealthClassification.MinorDegradation => 70,
            HealthClassification.SignificantDegradation => 50,
            HealthClassification.FailureImminent => 25,
            _ => 0
        };
    }
}

/// <summary>
/// DTO for dashboard statistics response.
/// </summary>
public class DashboardStatsDto
{
    /// <summary>
    /// Total number of machines.
    /// </summary>
    public int TotalMachines { get; set; }
    
    /// <summary>
    /// Number of machines currently running (Operational).
    /// </summary>
    public int RunningMachines { get; set; }
    
    /// <summary>
    /// Number of machines under maintenance.
    /// </summary>
    public int MaintenanceMachines { get; set; }
    
    /// <summary>
    /// Number of machines in critical/error state.
    /// </summary>
    public int ErrorMachines { get; set; }
    
    /// <summary>
    /// Average health score across all machines (0-100).
    /// </summary>
    public double AverageHealthScore { get; set; }
    
    /// <summary>
    /// Number of active alerts.
    /// </summary>
    public int ActiveAlerts { get; set; }
    
    /// <summary>
    /// Number of machines pending maintenance.
    /// </summary>
    public int PendingMaintenance { get; set; }
}
