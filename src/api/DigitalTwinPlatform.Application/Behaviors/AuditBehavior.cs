using MediatR;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.Behaviors;

/// <summary>
/// Pipeline behavior for auditing command execution.
/// Logs all command executions with user information and request/response data.
/// </summary>
public class AuditBehavior<TRequest, TResponse>(ILogger<AuditBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IAuditableCommand
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var commandName = typeof(TRequest).Name;
        var userId = request.UserId;
        var timestamp = DateTime.UtcNow;

        logger.LogInformation(
            "Audit: Command {CommandName} started by user {UserId} at {Timestamp}",
            commandName,
            userId ?? "Unknown",
            timestamp);

        try
        {
            var response = await next();

            logger.LogInformation(
                "Audit: Command {CommandName} completed successfully by user {UserId} at {Timestamp}. Request: {@Request}",
                commandName,
                userId ?? "Unknown",
                DateTime.UtcNow,
                request);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Audit: Command {CommandName} failed for user {UserId} at {Timestamp}. Request: {@Request}",
                commandName,
                userId ?? "Unknown",
                DateTime.UtcNow,
                request);
            throw;
        }
    }
}

/// <summary>
/// Marker interface for commands that should be audited.
/// </summary>
public interface IAuditableCommand
{
    string? UserId { get; }
}
