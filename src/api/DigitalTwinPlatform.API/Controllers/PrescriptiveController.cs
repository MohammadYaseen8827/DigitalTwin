using DigitalTwinPlatform.Application.Maintenance;
using DigitalTwinPlatform.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptiveController : ControllerBase
{
    private readonly IPrescriptiveService _prescriptiveService;

    public PrescriptiveController(IPrescriptiveService prescriptiveService)
    {
        _prescriptiveService = prescriptiveService;
    }

    [HttpGet("{machineId}/analysis")]
    public async Task<ActionResult<IEnumerable<MaintenanceWindow>>> GetAnalysis(Guid machineId, [FromQuery] int days = 30)
    {
        var results = await _prescriptiveService.RunWhatIfAnalysisAsync(machineId, days);
        return Ok(results);
    }

    [HttpGet("{machineId}/optimal")]
    public async Task<ActionResult<MaintenanceWindow>> GetOptimal(Guid machineId)
    {
        var result = await _prescriptiveService.GetOptimalMaintenanceDateAsync(machineId);
        return Ok(result);
    }
}
