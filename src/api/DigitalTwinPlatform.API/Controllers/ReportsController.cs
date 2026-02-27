using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DigitalTwinPlatform.Application.Abstractions.Services;
using DigitalTwinPlatform.Application.Reports.Models;

namespace DigitalTwinPlatform.API.Controllers;

/// <summary>
/// Controller for generating and managing reports
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ILogger<ReportsController> _logger;
    private readonly IReportService _reportService;

    public ReportsController(
        ILogger<ReportsController> logger,
        IReportService reportService)
    {
        _logger = logger;
        _reportService = reportService;
    }

    /// <summary>
    /// Generate a report based on template and parameters
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateReport(
        [FromBody] ReportGenerationRequest request,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(request.TemplateId))
            {
                return BadRequest("Template ID is required");
            }

            var response = await _reportService.GenerateReportAsync(request, ct);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report");
            return StatusCode(500, "Internal server error while generating report");
        }
    }

    /// <summary>
    /// Export data in specified format
    /// </summary>
    [HttpPost("export")]
    public async Task<IActionResult> ExportData(
        [FromBody] ExportDataRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var content = await _reportService.ExportDataAsync(request, ct);
            var fileName = request.FileName ?? $"export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{request.Format}";
            
            var contentType = request.Format.ToLower() switch
            {
                "csv" => "text/csv",
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "pdf" => "application/pdf",
                "json" => "application/json",
                _ => "application/octet-stream"
            };

            return File(content, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting data");
            return StatusCode(500, "Internal server error while exporting data");
        }
    }

    /// <summary>
    /// Get available report templates
    /// </summary>
    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates(CancellationToken ct = default)
    {
        try
        {
            var templates = await _reportService.GetTemplatesAsync(ct);
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving report templates");
            return StatusCode(500, "Internal server error while retrieving templates");
        }
    }

    /// <summary>
    /// Schedule automated report generation
    /// </summary>
    [HttpPost("schedule")]
    public async Task<IActionResult> ScheduleReport(
        [FromBody] ReportSchedule schedule,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(schedule.CronExpression))
            {
                return BadRequest("Cron expression is required");
            }

            var createdSchedule = await _reportService.ScheduleReportAsync(schedule, ct);
            return Ok(createdSchedule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling report");
            return StatusCode(500, "Internal server error while scheduling report");
        }
    }

    /// <summary>
    /// Get report generation history
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        try
        {
            var history = await _reportService.GetHistoryAsync(page, pageSize, ct);
            return Ok(new { Items = history, Page = page, PageSize = pageSize });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving report history");
            return StatusCode(500, "Internal server error while retrieving history");
        }
    }

    /// <summary>
    /// Download generated report
    /// </summary>
    [HttpGet("download/{reportId}")]
    public async Task<IActionResult> DownloadReport(
        string reportId,
        CancellationToken ct = default)
    {
        try
        {
            var (content, contentType, fileName) = await _reportService.DownloadReportAsync(reportId, ct);
            return File(content, contentType, fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading report {ReportId}", reportId);
            return StatusCode(500, "Internal server error while downloading report");
        }
    }
}
