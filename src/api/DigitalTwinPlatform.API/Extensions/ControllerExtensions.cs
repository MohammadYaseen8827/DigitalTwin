using DigitalTwinPlatform.API.Models;
using DigitalTwinPlatform.API.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Extensions;

public static class ControllerExtensions
{
    public static ActionResult<T> NotFoundError<T>(
        this ControllerBase controller,
        string resourceType,
        string resourceId,
        string? details = null)
    {
        var error = new ApiErrorDto
        {
            ErrorCode = ErrorCodes.ResourceNotFound,
            Message = $"{resourceType} not found",
            Details = details ?? $"The requested {resourceType} with ID '{resourceId}' was not found.",
            RequestId = controller.HttpContext.TraceIdentifier
        };

        return controller.NotFound(new ErrorResponse { Error = error, StatusCode = 404 });
    }

    public static ActionResult<T> BadRequestError<T>(
        this ControllerBase controller,
        string message,
        string? details = null,
        List<ValidationError>? validationErrors = null)
    {
        var error = new ApiErrorDto
        {
            ErrorCode = ErrorCodes.BadRequest,
            Message = message,
            Details = details,
            RequestId = controller.HttpContext.TraceIdentifier,
            ValidationErrors = validationErrors
        };

        return controller.BadRequest(new ErrorResponse { Error = error, StatusCode = 400 });
    }

    public static ActionResult<T> ConflictError<T>(
        this ControllerBase controller,
        string message,
        string? details = null)
    {
        var error = new ApiErrorDto
        {
            ErrorCode = ErrorCodes.Conflict,
            Message = message,
            Details = details,
            RequestId = controller.HttpContext.TraceIdentifier
        };

        return controller.Conflict(new ErrorResponse { Error = error, StatusCode = 409 });
    }
}
