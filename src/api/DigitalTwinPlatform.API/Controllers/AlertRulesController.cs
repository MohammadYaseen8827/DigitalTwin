using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DigitalTwinPlatform.API.Controllers;

/// <summary>
/// API controller for managing alert rules.
/// Provides CRUD operations for alert threshold rules and notification configurations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[ApiVersion("1.0")]
public class AlertRulesController : ControllerBase
{
    private static readonly List<AlertRuleDto> _alertRules = new();
    private static int _nextId = 1;

    /// <summary>
    /// Gets all alert rules.
    /// </summary>
    /// <returns>List of all alert rules.</returns>
    /// <response code="200">Returns alert rules successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AlertRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IEnumerable<AlertRuleDto>> GetAllRules()
    {
        return Ok(_alertRules);
    }

    /// <summary>
    /// Gets a specific alert rule by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the alert rule.</param>
    /// <returns>The alert rule details.</returns>
    /// <response code="200">Returns alert rule successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="404">Alert rule not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AlertRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AlertRuleDto> GetRule(int id)
    {
        var rule = _alertRules.FirstOrDefault(r => r.Id == id);
        if (rule == null)
        {
            return NotFound(new { Message = $"Alert rule with ID {id} not found" });
        }
        return Ok(rule);
    }

    /// <summary>
    /// Creates a new alert rule.
    /// </summary>
    /// <param name="request">The alert rule creation request.</param>
    /// <returns>The created alert rule.</returns>
    /// <response code="201">Alert rule created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(AlertRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<AlertRuleDto> CreateRule([FromBody] AlertRuleCreateDto request)
    {
        if (string.IsNullOrEmpty(request.Name))
        {
            return BadRequest(new { Message = "Rule name is required" });
        }

        var rule = new AlertRuleDto
        {
            Id = _nextId++,
            Name = request.Name,
            Description = request.Description,
            Enabled = request.Enabled,
            Severity = MapSeverity(request.Severity),
            Condition = new AlertConditionDto
            {
                Type = request.Condition.Type,
                Sensor = request.Condition.Sensor,
                Threshold = request.Condition.Threshold,
                Operator = request.Condition.Operator
            },
            NotificationChannels = request.NotificationChannels,
            EscalationEnabled = request.EscalationEnabled,
            EscalationDelay = request.EscalationDelay,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _alertRules.Add(rule);

        return CreatedAtAction(nameof(GetRule), new { id = rule.Id }, rule);
    }

    /// <summary>
    /// Updates an existing alert rule.
    /// </summary>
    /// <param name="id">The unique identifier of the alert rule.</param>
    /// <param name="request">The alert rule update request.</param>
    /// <returns>The updated alert rule.</returns>
    /// <response code="200">Alert rule updated successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="404">Alert rule not found.</response>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(AlertRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AlertRuleDto> UpdateRule(int id, [FromBody] AlertRuleUpdateDto request)
    {
        var rule = _alertRules.FirstOrDefault(r => r.Id == id);
        if (rule == null)
        {
            return NotFound(new { Message = $"Alert rule with ID {id} not found" });
        }

        if (!string.IsNullOrEmpty(request.Name))
            rule.Name = request.Name;
        if (request.Description != null)
            rule.Description = request.Description;
        if (request.Enabled.HasValue)
            rule.Enabled = request.Enabled.Value;
        if (!string.IsNullOrEmpty(request.Severity))
            rule.Severity = MapSeverity(request.Severity);
        if (request.Condition != null)
        {
            rule.Condition.Type = request.Condition.Type;
            rule.Condition.Sensor = request.Condition.Sensor;
            rule.Condition.Threshold = request.Condition.Threshold;
            rule.Condition.Operator = request.Condition.Operator;
        }
        if (request.NotificationChannels != null)
            rule.NotificationChannels = request.NotificationChannels;
        if (request.EscalationEnabled.HasValue)
            rule.EscalationEnabled = request.EscalationEnabled.Value;
        if (request.EscalationDelay.HasValue)
            rule.EscalationDelay = request.EscalationDelay.Value;

        rule.UpdatedAt = DateTime.UtcNow;

        return Ok(rule);
    }

    /// <summary>
    /// Deletes an alert rule.
    /// </summary>
    /// <param name="id">The unique identifier of the alert rule.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <response code="204">Alert rule deleted successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="404">Alert rule not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteRule(int id)
    {
        var rule = _alertRules.FirstOrDefault(r => r.Id == id);
        if (rule == null)
        {
            return NotFound(new { Message = $"Alert rule with ID {id} not found" });
        }

        _alertRules.Remove(rule);
        return NoContent();
    }

    /// <summary>
    /// Toggles an alert rule's enabled status.
    /// </summary>
    /// <param name="id">The unique identifier of the alert rule.</param>
    /// <param name="request">The toggle request containing the new enabled status.</param>
    /// <returns>The updated alert rule.</returns>
    /// <response code="200">Alert rule status updated successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="404">Alert rule not found.</response>
    [HttpPatch("{id:int}/toggle")]
    [ProducesResponseType(typeof(AlertRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AlertRuleDto> ToggleRule(int id, [FromBody] ToggleRuleRequest request)
    {
        var rule = _alertRules.FirstOrDefault(r => r.Id == id);
        if (rule == null)
        {
            return NotFound(new { Message = $"Alert rule with ID {id} not found" });
        }

        rule.Enabled = request.Enabled;
        rule.UpdatedAt = DateTime.UtcNow;

        return Ok(rule);
    }

    private static string MapSeverity(string severity)
    {
        return severity?.ToLowerInvariant() switch
        {
            "critical" => "critical",
            "warning" => "warning",
            "error" => "error",
            _ => "info"
        };
    }
}

/// <summary>
/// DTO for alert rule response.
/// </summary>
public class AlertRuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string Severity { get; set; } = "warning";
    public AlertConditionDto Condition { get; set; } = new();
    public List<string> NotificationChannels { get; set; } = new();
    public bool EscalationEnabled { get; set; }
    public int? EscalationDelay { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for alert condition.
/// </summary>
public class AlertConditionDto
{
    public string Type { get; set; } = "threshold";
    public string? Sensor { get; set; }
    public double? Threshold { get; set; }
    public string? Operator { get; set; }
}

/// <summary>
/// DTO for creating an alert rule.
/// </summary>
public class AlertRuleCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public string Severity { get; set; } = "warning";
    public AlertConditionDto Condition { get; set; } = new();
    public List<string> NotificationChannels { get; set; } = new();
    public bool EscalationEnabled { get; set; }
    public int? EscalationDelay { get; set; }
}

/// <summary>
/// DTO for updating an alert rule.
/// </summary>
public class AlertRuleUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? Enabled { get; set; }
    public string? Severity { get; set; }
    public AlertConditionDto? Condition { get; set; }
    public List<string>? NotificationChannels { get; set; }
    public bool? EscalationEnabled { get; set; }
    public int? EscalationDelay { get; set; }
}

/// <summary>
/// DTO for toggling rule status.
/// </summary>
public class ToggleRuleRequest
{
    public bool Enabled { get; set; }
}
