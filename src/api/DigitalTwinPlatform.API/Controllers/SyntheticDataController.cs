using DigitalTwinPlatform.Application.Services;
using DigitalTwinPlatform.Application.Simulations.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace DigitalTwinPlatform.API.Controllers;

/// <summary>
/// API controller for synthetic data generation and validation.
/// Provides high-fidelity synthetic sensor data generation with statistical validation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class SyntheticDataController : ControllerBase
{
    private readonly ISyntheticDataGenerator _syntheticDataGenerator;
    private readonly ILogger<SyntheticDataController> _logger;

    public SyntheticDataController(
        ISyntheticDataGenerator syntheticDataGenerator,
        ILogger<SyntheticDataController> logger)
    {
        _syntheticDataGenerator = syntheticDataGenerator;
        _logger = logger;
    }

    /// <summary>
    /// Generate synthetic telemetry data for a machine type.
    /// </summary>
    /// <param name="request">Synthetic data generation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Returns the generation result.</response>
    [HttpPost("generate")]
    public async Task<ActionResult<DigitalTwinPlatform.Domain.Entities.SyntheticDataGeneration>> GenerateSyntheticData(
        [FromBody] SyntheticDataGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting synthetic data generation for machine type {MachineType}, trajectories: {NumberOfTrajectories}", 
                request.MachineType, request.NumberOfTrajectories);

            var result = await _syntheticDataGenerator.GenerateSyntheticDataAsync(request, cancellationToken);

            _logger.LogInformation("Successfully generated {DataPointCount} synthetic data points for machine type {MachineType}", 
                result.Statistics.TotalDataPoints, request.MachineType);

            return CreatedAtAction("GetSyntheticData", new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate synthetic data for machine type {MachineType}", request.MachineType);
            return StatusCode(500, new { error = "Internal server error during data generation" });
        }
    }

    /// <summary>
    /// Validate synthetic data against benchmark datasets.
    /// </summary>
    /// <param name="request">Validation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns validation results.</response>
    [HttpPost("validate")]
    public async Task<ActionResult<DigitalTwinPlatform.Domain.Entities.DataValidationReport>> ValidateSyntheticData(
        [FromBody] ValidateSyntheticDataRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting synthetic data validation for machine type {MachineType}", request.MachineType);

            var report = await _syntheticDataGenerator.ValidateSyntheticDataAsync(request.SyntheticData, request.MachineType, cancellationToken);

            _logger.LogInformation("Successfully validated synthetic data for machine type {MachineType} with overall score: {OverallScore}", 
                request.MachineType, report.OverallScore);

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate synthetic data for machine type {MachineType}", request.MachineType);
            return StatusCode(500, new { error = "Internal server error during data validation" });
        }
    }

    /// <summary>
    /// Get generation statistics for a machine type.
    /// </summary>
    /// <param name="machineType">Machine type filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns generation statistics.</response>
    [HttpGet("statistics/{machineType}")]
    public async Task<ActionResult<DigitalTwinPlatform.Domain.Entities.GenerationStatistics>> GetGenerationStatistics(
        [FromRoute] string machineType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting generation statistics for machine type {MachineType}", machineType);

            var statistics = await _syntheticDataGenerator.GetGenerationStatisticsAsync(machineType, cancellationToken);

            _logger.LogInformation("Retrieved generation statistics for machine type {MachineType}: {TotalGenerations} generations", 
                machineType, statistics.TotalGenerations);

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get generation statistics for machine type {MachineType}", machineType);
            return StatusCode(500, new { error = "Internal server error retrieving statistics" });
        }
    }

    /// <summary>
    /// Get all generation statistics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns all generation statistics.</response>
    [HttpGet("statistics")]
    public async Task<ActionResult<List<DigitalTwinPlatform.Domain.Entities.GenerationStatistics>>> GetAllGenerationStatistics(
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        try
        {
            _logger.LogInformation("Getting all generation statistics");

            // For now, return empty list - in production this would aggregate from database
            var statistics = new List<DigitalTwinPlatform.Domain.Entities.GenerationStatistics>();

            _logger.LogInformation("Retrieved generation statistics for {Count} machine types", statistics.Count);

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all generation statistics");
            return StatusCode(500, new { error = "Internal server error retrieving statistics" });
        }
    }
}