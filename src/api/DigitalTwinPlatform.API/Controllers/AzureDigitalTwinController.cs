using DigitalTwinPlatform.API.Models;
using DigitalTwinPlatform.API.Services.Integration;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AzureDigitalTwinController(IAzureDigitalTwinService adtService) : ControllerBase
{
    [HttpPost("machines/{machineId:guid}/sync")]
    public async Task<IActionResult> SyncMachine(Guid machineId)
    {
        await adtService.SyncMachineAsync(machineId);
        return Accepted();
    }

    [HttpPost("production-lines/{lineId:guid}/sync")]
    public async Task<IActionResult> SyncProductionLine(Guid lineId)
    {
        await adtService.SyncProductionLineAsync(lineId);
        return Accepted();
    }

    [HttpPost("twins/upsert")]
    public async Task<IActionResult> UpsertTwin(AzureTwinUpsertDto dto)
    {
        await adtService.UpsertTwinAsync(dto.Id, dto.ModelId, dto.Properties);
        return Accepted();
    }
}
