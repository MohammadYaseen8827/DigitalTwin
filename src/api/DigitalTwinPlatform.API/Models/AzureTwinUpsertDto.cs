namespace DigitalTwinPlatform.API.Models;

public record AzureTwinUpsertDto(
    Guid Id,
    string ModelId,
    Dictionary<string, object> Properties);
