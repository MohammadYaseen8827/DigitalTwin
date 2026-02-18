using System.Text.Json;

namespace DigitalTwinPlatform.Domain.Entities;

public class ProductionLine
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public JsonDocument Configuration { get; set; } = JsonDocument.Parse("{}");
    public ICollection<Machine> Machines { get; set; } = new List<Machine>();
}
