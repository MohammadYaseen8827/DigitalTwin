using DigitalTwinPlatform.Application.Abstractions.Services;
using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Predictions.Models;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.Predictions.Commands;

public sealed record PredictCommand(PredictionRequestDto Request) : IRequest<PredictionDto>;

internal sealed class PredictCommandHandler(
    IPredictionService predictionService,
    IUnitOfWork unitOfWork,
    ILogger<PredictCommandHandler> logger)
    : IRequestHandler<PredictCommand, PredictionDto>
{
    public async Task<PredictionDto> Handle(PredictCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting prediction for machine {MachineId}", request.Request.MachineId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Get prediction from service
            var prediction = await predictionService.PredictAsync(request.Request, cancellationToken);

            // Persist prediction to database
            var repository = unitOfWork.Repository<Prediction>();
            
            var predictionEntity = new Prediction
            {
                Id = Guid.NewGuid(),
                MachineId = request.Request.MachineId,
                RemainingUsefulLifeDays = prediction.RemainingUsefulLifeDays,
                FailureProbability = prediction.FailureProbability,
                HealthStatus = prediction.HealthStatus,
                CreatedAt = DateTime.UtcNow
            };

            await repository.AddAsync(predictionEntity, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            logger.LogInformation("Prediction completed for machine {MachineId}", request.Request.MachineId);
            return prediction;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Prediction failed for machine {MachineId}", request.Request.MachineId);
            throw;
        }
    }
}
