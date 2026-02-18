using MediatR;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.Behaviors;

/// <summary>
/// Pipeline behavior for managing transactions across command handlers.
/// Ensures all database operations within a command are atomic.
/// </summary>
public class TransactionBehavior<TRequest, TResponse>(ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        try
        {
            logger.LogInformation("Beginning transaction for request: {RequestName}", requestName);
            var response = await next();
            logger.LogInformation("Transaction committed for request: {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Transaction rolled back for request: {RequestName}", requestName);
            throw;
        }
    }
}
