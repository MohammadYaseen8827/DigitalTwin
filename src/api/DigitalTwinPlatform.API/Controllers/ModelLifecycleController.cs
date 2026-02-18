using DigitalTwinPlatform.API.Services.Analytics;
using DigitalTwinPlatform.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModelLifecycleController : ControllerBase
{
    private readonly IModelLifecycleService _lifecycleService;
    private readonly ILogger<ModelLifecycleController> _logger;

    public ModelLifecycleController(
        IModelLifecycleService lifecycleService,
        ILogger<ModelLifecycleController> logger)
    {
        _lifecycleService = lifecycleService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new model version
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ModelVersionDto>> RegisterModelVersion(
        [FromBody] RegisterModelVersionRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var modelVersion = await _lifecycleService.RegisterModelVersionAsync(
                request.ModelType,
                request.ModelPath,
                request.Metrics,
                request.TrainingDatasetHash,
                request.Notes,
                ct);

            return Ok(MapToDto(modelVersion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering model version");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Promote a model version to a new status
    /// </summary>
    [HttpPost("{id}/promote")]
    public async Task<ActionResult<ModelVersionDto>> PromoteModel(
        Guid id,
        [FromBody] PromoteModelRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var modelVersion = await _lifecycleService.PromoteModelAsync(
                id,
                request.TargetStatus,
                request.Notes,
                ct);

            return Ok(MapToDto(modelVersion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error promoting model version");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all model versions, optionally filtered by model type
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ModelVersionDto>>> GetModelVersions(
        [FromQuery] string? modelType = null,
        CancellationToken ct = default)
    {
        try
        {
            var versions = await _lifecycleService.GetModelVersionsAsync(modelType, ct);
            return Ok(versions.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving model versions");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get the current production version for a model type
    /// </summary>
    [HttpGet("production/{modelType}")]
    public async Task<ActionResult<ModelVersionDto>> GetProductionVersion(
        string modelType,
        CancellationToken ct = default)
    {
        try
        {
            var version = await _lifecycleService.GetProductionVersionAsync(modelType, ct);
            if (version == null)
            {
                return NotFound(new { error = $"No production version found for {modelType}" });
            }

            return Ok(MapToDto(version));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving production version");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Compare two model versions
    /// </summary>
    [HttpPost("compare")]
    public async Task<ActionResult<ModelComparisonResult>> CompareModels(
        [FromBody] CompareModelsRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var comparison = await _lifecycleService.CompareModelsAsync(
                request.ModelVersionId1,
                request.ModelVersionId2,
                ct);

            return Ok(comparison);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing models");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get performance summary for a model version
    /// </summary>
    [HttpGet("{id}/performance")]
    public async Task<ActionResult<ModelPerformanceSummary>> GetModelPerformance(
        Guid id,
        CancellationToken ct = default)
    {
        try
        {
            var performance = await _lifecycleService.GetModelPerformanceAsync(id, ct);
            return Ok(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving model performance");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private static ModelVersionDto MapToDto(DigitalTwinPlatform.Domain.Entities.ModelVersion version)
    {
        return new ModelVersionDto
        {
            Id = version.Id,
            ModelType = version.ModelType,
            Version = version.Version,
            ModelPath = version.ModelPath,
            TrainedAt = version.TrainedAt,
            Metrics = version.Metrics,
            TrainingDatasetHash = version.TrainingDatasetHash,
            Status = version.Status.ToString(),
            PromotedAt = version.PromotedAt,
            Notes = version.Notes,
            CreatedAt = version.CreatedAt,
            UpdatedAt = version.UpdatedAt
        };
    }
}

public class RegisterModelVersionRequest
{
    public string ModelType { get; set; } = string.Empty;
    public string ModelPath { get; set; } = string.Empty;
    public Dictionary<string, double> Metrics { get; set; } = new();
    public string? TrainingDatasetHash { get; set; }
    public string? Notes { get; set; }
}

public class PromoteModelRequest
{
    public ModelStatus TargetStatus { get; set; }
    public string? Notes { get; set; }
}

public class CompareModelsRequest
{
    public Guid ModelVersionId1 { get; set; }
    public Guid ModelVersionId2 { get; set; }
}

public class ModelVersionDto
{
    public Guid Id { get; set; }
    public string ModelType { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string ModelPath { get; set; } = string.Empty;
    public DateTime TrainedAt { get; set; }
    public Dictionary<string, double> Metrics { get; set; } = new();
    public string? TrainingDatasetHash { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PromotedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
