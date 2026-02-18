using System.Linq;
using DigitalTwinPlatform.Application.ProductionLines.Models;
using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.Application.ProductionLines;

internal static class ProductionLineMapper
{
    public static ProductionLineDto ToDto(ProductionLine productionLine) => new(
        productionLine.Id,
        productionLine.Name,
        productionLine.Configuration,
        productionLine.Machines.Select(m => m.Id));
}
