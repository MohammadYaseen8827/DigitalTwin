using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.Behaviors;

public class ExceptionHandlingBehavior<TRequest, TResponse>(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Validation failed for request: {RequestName}", typeof(TRequest).Name);
            throw;
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Resource not found for request: {RequestName}", typeof(TRequest).Name);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid operation for request: {RequestName}", typeof(TRequest).Name);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in request: {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}
