namespace DigitalTwinPlatform.API.Models;

public record HealthStatusDto(
    string Overall,
    DateTime Timestamp,
    string DatabaseStatus,
    string SignalRStatus,
    string AzureDigitalTwinsStatus);
