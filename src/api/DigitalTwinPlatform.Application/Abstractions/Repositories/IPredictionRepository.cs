using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.Application.Abstractions.Repositories;

public interface IPredictionRepository : IRepository<Prediction>
{
    Task<IEnumerable<Prediction>> GetByMachineIdAsync(Guid machineId, int take = 100, CancellationToken ct = default);
    Task<IEnumerable<Prediction>> SearchAsync(string query, Guid? machineId = null, CancellationToken ct = default);
}
