using System.Linq;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Predictions.Models;
using DigitalTwinPlatform.Domain.Entities;
using MediatR;

namespace DigitalTwinPlatform.Application.Predictions.Queries;

public sealed record GetPredictionHistoryQuery(Guid MachineId, int Take = 100) : IRequest<IReadOnlyCollection<PredictionDto>>;

internal sealed class GetPredictionHistoryQueryHandler(IRepository<Prediction> repository)
    : IRequestHandler<GetPredictionHistoryQuery, IReadOnlyCollection<PredictionDto>>
{
    public async Task<IReadOnlyCollection<PredictionDto>> Handle(GetPredictionHistoryQuery request, CancellationToken cancellationToken)
    {
        var predictions = await repository.GetAllAsync(p => p.MachineId == request.MachineId);
        return predictions
            .OrderByDescending(p => p.CreatedAt)
            .Take(request.Take)
            .Select(PredictionMapper.ToDto)
            .ToList();
    }
}
