using DigitalTwinPlatform.Application.Services;
using DigitalTwinPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MachineConfigurationController : ControllerBase
{
    private readonly IMachineConfigurationService _configurationService;
    private readonly ILogger<MachineConfigurationController> _logger;

    public MachineConfigurationController(
        IMachineConfigurationService configurationService,
        ILogger<MachineConfigurationController> logger)
    {
        _configurationService = configurationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all machine configurations
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<MachineConfiguration>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MachineConfiguration>>> GetAllConfigurations(CancellationToken ct = default)
    {
        var configurations = await _configurationService.LoadAllConfigurationsAsync(ct);
        return Ok(configurations);
    }

    /// <summary>
    /// Get configuration for a specific machine type
    /// </summary>
    [HttpGet("{machineType}")]
    [ProducesResponseType(typeof(MachineConfiguration), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MachineConfiguration>> GetConfiguration(string machineType, CancellationToken ct = default)
    {
        try
        {
            var configuration = await _configurationService.LoadConfigurationAsync(machineType, ct);
            return Ok(configuration);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { message = $"Configuration not found for machine type: {machineType}" });
        }
    }

    /// <summary>
    /// Create or update a machine configuration
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateOrUpdateConfiguration(
        [FromBody] MachineConfiguration configuration,
        CancellationToken ct = default)
    {
        if (configuration == null || string.IsNullOrWhiteSpace(configuration.MachineType))
        {
            return BadRequest(new { message = "Invalid configuration. MachineType is required." });
        }

        try
        {
            await _configurationService.SaveConfigurationAsync(configuration.MachineType, configuration, ct);
            return CreatedAtAction(
                nameof(GetConfiguration),
                new { machineType = configuration.MachineType },
                configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration for machine type: {MachineType}", configuration.MachineType);
            return BadRequest(new { message = $"Failed to save configuration: {ex.Message}" });
        }
    }

    /// <summary>
    /// Delete a machine configuration
    /// </summary>
    [HttpDelete("{machineType}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteConfiguration(string machineType, CancellationToken ct = default)
    {
        var exists = await _configurationService.ConfigurationExistsAsync(machineType, ct);
        if (!exists)
        {
            return NotFound(new { message = $"Configuration not found for machine type: {machineType}" });
        }

        // Note: The service doesn't have a delete method yet, but we can implement file deletion here
        // For now, return NotImplemented
        return StatusCode(StatusCodes.Status501NotImplemented, 
            new { message = "Delete functionality not yet implemented" });
    }

    /// <summary>
    /// Validate a JSON configuration without saving it
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ValidateConfiguration(
        [FromBody] string jsonContent,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return BadRequest(new { message = "JSON content is required" });
        }

        var isValid = await _configurationService.ValidateConfigurationAsync(jsonContent, ct);
        
        if (isValid)
        {
            return Ok(new { valid = true, message = "Configuration is valid" });
        }
        else
        {
            return BadRequest(new { valid = false, message = "Configuration is invalid" });
        }
    }
}
