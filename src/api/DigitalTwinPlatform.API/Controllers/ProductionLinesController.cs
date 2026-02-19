using DigitalTwinPlatform.Application.ProductionLines.Commands;
using DigitalTwinPlatform.Application.ProductionLines.Models;
using DigitalTwinPlatform.Application.ProductionLines.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductionLinesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductionLineDto>>> Get(CancellationToken ct)
        => Ok(await mediator.Send(new GetProductionLinesQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductionLineDto>> Get(Guid id, CancellationToken ct)
        => Ok(await mediator.Send(new GetProductionLineQuery(id), ct));

    [HttpPost]
    public async Task<ActionResult<ProductionLineDto>> Create(ProductionLineCreateDto dto, CancellationToken ct)
    {
        var created = await mediator.Send(new CreateProductionLineCommand(dto), ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductionLineDto>> Update(Guid id, ProductionLineUpdateDto dto, CancellationToken ct)
        => Ok(await mediator.Send(new UpdateProductionLineCommand(id, dto), ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductionLineCommand(id), ct);
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductionLineDto>>> Search([FromQuery] string query, CancellationToken ct)
    {
        var result = await mediator.Send(new SearchProductionLinesQuery(query), ct);
        return Ok(result);
    }
}
