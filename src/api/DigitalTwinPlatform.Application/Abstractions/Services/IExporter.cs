
namespace DigitalTwinPlatform.Application.Abstractions.Services;

public interface IExporter
{
    Task ExportAsync<T>(IEnumerable<T> data, string destinationPath, CancellationToken ct = default);
}
