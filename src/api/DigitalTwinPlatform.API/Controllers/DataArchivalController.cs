using DigitalTwinPlatform.API.Models;
using DigitalTwinPlatform.API.Services.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DataArchivalController(IDataArchivalService archivalService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Archive(DataArchivalRequestDto dto, CancellationToken ct)
    {
        await archivalService.ArchiveOldTelemetryAsync(dto.RetentionDays, ct);
        return Accepted();
    }
}
