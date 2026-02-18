using MediatR;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.Behaviors;

/// <summary>
/// Pipeline behavior for authorization checks on commands.
/// Ensures only authorized users can execute specific commands.
/// </summary>
public class AuthorizationBehavior<TRequest, TResponse>(ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IAuthorizableCommand
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var commandName = typeof(TRequest).Name;
        var requiredRole = request.RequiredRole;

        if (string.IsNullOrEmpty(requiredRole))
        {
            logger.LogDebug("Command {CommandName} has no role requirement", commandName);
            return await next();
        }

        var userRole = request.UserRole;
        
        if (string.IsNullOrEmpty(userRole))
        {
            logger.LogWarning("Authorization failed for command {CommandName}: User has no role", commandName);
            throw new UnauthorizedAccessException($"User is not authorized to execute {commandName}");
        }

        if (!IsUserAuthorized(userRole, requiredRole))
        {
            logger.LogWarning(
                "Authorization failed for command {CommandName}: User role {UserRole} does not have required role {RequiredRole}",
                commandName,
                userRole,
                requiredRole);
            throw new UnauthorizedAccessException($"User role '{userRole}' is not authorized to execute {commandName}");
        }

        logger.LogInformation(
            "Authorization successful for command {CommandName}: User role {UserRole} has required role {RequiredRole}",
            commandName,
            userRole,
            requiredRole);

        return await next();
    }

    private static bool IsUserAuthorized(string userRole, string requiredRole)
    {
        // Simple role-based authorization
        // Can be extended with role hierarchy or permission-based authorization
        return userRole.Equals(requiredRole, StringComparison.OrdinalIgnoreCase) ||
               IsAdminRole(userRole);
    }

    private static bool IsAdminRole(string role)
    {
        return role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
               role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Marker interface for commands that require authorization.
/// </summary>
public interface IAuthorizableCommand
{
    string? RequiredRole { get; }
    string? UserRole { get; }
}
