using MediatR;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Executing request: {RequestName}", requestName);

        try
        {
            var response = await next();
            logger.LogInformation("Successfully executed request: {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing request: {RequestName}", requestName);
            throw;
        }
    }
}
