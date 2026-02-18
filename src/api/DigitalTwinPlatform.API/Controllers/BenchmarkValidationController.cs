using DigitalTwinPlatform.API.Services.Analytics;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BenchmarkValidationController : ControllerBase
{
    private readonly IBenchmarkValidationService _validationService;
    private readonly ILogger<BenchmarkValidationController> _logger;

    public BenchmarkValidationController(
        IBenchmarkValidationService validationService,
        ILogger<BenchmarkValidationController> logger)
    {
        _validationService = validationService;
        _logger = logger;
    }

    /// <summary>
    /// Validate a model version against a benchmark dataset
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<BenchmarkValidationResult>> ValidateModel(
        [FromBody] ValidateModelRequest request,
        CancellationToken ct = default)
    {
        try
        {
            BenchmarkValidationResult result;

            if (!string.IsNullOrEmpty(request.ModelVersionId))
            {
                result = await _validationService.ValidateAgainstBenchmarkAsync(
                    request.BenchmarkDataset,
                    request.ModelVersionId,
                    ct);
            }
            else if (!string.IsNullOrEmpty(request.ModelPath))
            {
                result = await _validationService.ValidateModelAsync(
                    request.BenchmarkDataset,
                    request.ModelType ?? "RUL",
                    request.ModelPath,
                    ct);
            }
            else
            {
                return BadRequest(new { error = "Either ModelVersionId or ModelPath must be provided" });
            }

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                return BadRequest(new { error = result.ErrorMessage });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating model against benchmark");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get available benchmark datasets
    /// </summary>
    [HttpGet("benchmarks")]
    public async Task<ActionResult<List<string>>> GetAvailableBenchmarks(
        CancellationToken ct = default)
    {
        try
        {
            var benchmarks = await _validationService.GetAvailableBenchmarksAsync(ct);
            return Ok(benchmarks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available benchmarks");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get information about a benchmark dataset
    /// </summary>
    [HttpGet("benchmarks/{datasetName}")]
    public async Task<ActionResult<BenchmarkDatasetInfo>> GetBenchmarkInfo(
        string datasetName,
        CancellationToken ct = default)
    {
        try
        {
            var info = await _validationService.GetBenchmarkInfoAsync(datasetName, ct);
            return Ok(info);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving benchmark info");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class ValidateModelRequest
{
    public string BenchmarkDataset { get; set; } = string.Empty;
    public string? ModelVersionId { get; set; }
    public string? ModelPath { get; set; }
    public string? ModelType { get; set; }
}
