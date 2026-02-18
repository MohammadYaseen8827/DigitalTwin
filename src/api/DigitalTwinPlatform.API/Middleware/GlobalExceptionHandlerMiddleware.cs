using System.Net;
using System.Text.Json;
using DigitalTwinPlatform.API.Models;
using DigitalTwinPlatform.Domain.Common;

namespace DigitalTwinPlatform.API.Middleware;

public class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger,
    IWebHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var requestId = context.TraceIdentifier;
        var errorResponse = CreateErrorResponse(exception, requestId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = errorResponse.StatusCode;

        // Log the exception
        LogException(exception, errorResponse.Error.ErrorCode, requestId);

        // Serialize and return error response
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = environment.IsDevelopment()
        };

        var json = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await context.Response.WriteAsync(json);
    }

    /// <summary>
    /// Creates an appropriate error response based on the exception type.
    /// </summary>
    private ErrorResponse CreateErrorResponse(Exception exception, string requestId)
    {
        return exception switch
        {
            DomainValidationException domainValidation => CreateDomainValidationError(domainValidation, requestId),
            BusinessRuleViolationException businessRule => CreateBusinessRuleError(businessRule, requestId),
            DomainException domainException => CreateDomainError(domainException, requestId),
            KeyNotFoundException keyNotFound => CreateNotFoundError(keyNotFound, requestId),
            ArgumentException argumentException => CreateArgumentError(argumentException, requestId),
            InvalidOperationException invalidOp => CreateInvalidOperationError(invalidOp, requestId),
            UnauthorizedAccessException unauthorized => CreateUnauthorizedError(unauthorized, requestId),
            _ => CreateGenericError(exception, requestId)
        };
    }

    /// <summary>
    /// Creates an error response for domain validation exceptions.
    /// </summary>
    private static ErrorResponse CreateDomainValidationError(DomainValidationException exception, string requestId)
    {
        return new ErrorResponse
        {
            Error = new ApiErrorDto
            {
                ErrorCode = ErrorCodes.ValidationError,
                Message = "Validation failed",
                Details = exception.Message,
                RequestId = requestId,
                ValidationErrors = new List<ValidationError>
                {
                    new ValidationError
                    {
                        PropertyName = exception.PropertyName,
                        ErrorMessage = exception.Message,
                        AttemptedValue = exception.AttemptedValue
                    }
                }
            },
            StatusCode = (int)HttpStatusCode.BadRequest
        };
    }

    /// <summary>
    /// Creates an error response for business rule violations.
    /// </summary>
    private static ErrorResponse CreateBusinessRuleError(BusinessRuleViolationException exception, string requestId)
    {
        return new ErrorResponse
        {
            Error = new ApiErrorDto
            {
                ErrorCode = ErrorCodes.BusinessRuleViolation,
                Message = "Business rule violation",
                Details = exception.Message,
                RequestId = requestId,
                Metadata = new Dictionary<string, object>
                {
                    ["RuleName"] = exception.RuleName
                }
            },
            StatusCode = (int)HttpStatusCode.BadRequest
        };
    }

    /// <summary>
    /// Creates an error response for domain exceptions.
    /// </summary>
    private static ErrorResponse CreateDomainError(DomainException exception, string requestId)
    {
        return new ErrorResponse
        {
            Error = new ApiErrorDto
            {
                ErrorCode = ErrorCodes.InvalidOperation,
                Message = "Domain error occurred",
                Details = exception.Message,
                RequestId = requestId
            },
            StatusCode = (int)HttpStatusCode.BadRequest
        };
    }

    /// <summary>
    /// Creates an error response for resource not found exceptions.
    /// </summary>
    private static ErrorResponse CreateNotFoundError(KeyNotFoundException exception, string requestId)
    {
        return new ErrorResponse
        {
            Error = new ApiErrorDto
            {
                ErrorCode = ErrorCodes.ResourceNotFound,
                Message = "Resource not found",
                Details = exception.Message,
                RequestId = requestId
            },
            StatusCode = (int)HttpStatusCode.NotFound
        };
    }

    /// <summary>
    /// Creates an error response for argument exceptions.
    /// </summary>
    private static ErrorResponse CreateArgumentError(ArgumentException exception, string requestId)
    {
        return new ErrorResponse
        {
            Error = new ApiErrorDto
            {
                ErrorCode = ErrorCodes.BadRequest,
                Message = "Invalid argument",
                Details = exception.Message,
                RequestId = requestId
            },
            StatusCode = (int)HttpStatusCode.BadRequest
        };
    }

    /// <summary>
    /// Creates an error response for invalid operation exceptions.
    /// </summary>
    private static ErrorResponse CreateInvalidOperationError(InvalidOperationException exception, string requestId)
    {
        return new ErrorResponse
        {
            Error = new ApiErrorDto
            {
                ErrorCode = ErrorCodes.InvalidOperation,
                Message = "Invalid operation",
                Details = exception.Message,
                RequestId = requestId
            },
            StatusCode = (int)HttpStatusCode.BadRequest
        };
    }

    /// <summary>
    /// Creates an error response for unauthorized access exceptions.
    /// </summary>
    private static ErrorResponse CreateUnauthorizedError(UnauthorizedAccessException exception, string requestId)
    {
        return new ErrorResponse
        {
            Error = new ApiErrorDto
            {
                ErrorCode = ErrorCodes.Unauthorized,
                Message = "Unauthorized access",
                Details = exception.Message,
                RequestId = requestId
            },
            StatusCode = (int)HttpStatusCode.Unauthorized
        };
    }

    /// <summary>
    /// Creates a generic error response for unhandled exceptions.
    /// </summary>
    private ErrorResponse CreateGenericError(Exception exception, string requestId)
    {
        return new ErrorResponse
        {
            Error = new ApiErrorDto
            {
                ErrorCode = ErrorCodes.InternalServerError,
                Message = "An error occurred while processing your request",
                Details = environment.IsDevelopment() ? exception.ToString() : null,
                RequestId = requestId,
                Metadata = environment.IsDevelopment()
                    ? new Dictionary<string, object>
                    {
                        ["ExceptionType"] = exception.GetType().Name,
                        ["StackTrace"] = exception.StackTrace ?? string.Empty
                    }
                    : new Dictionary<string, object>()
            },
            StatusCode = (int)HttpStatusCode.InternalServerError
        };
    }

    private void LogException(Exception exception, string errorCode, string requestId)
    {
        var logLevel = exception switch
        {
            DomainValidationException => LogLevel.Warning,
            BusinessRuleViolationException => LogLevel.Warning,
            KeyNotFoundException => LogLevel.Warning,
            ArgumentException => LogLevel.Warning,
            InvalidOperationException => LogLevel.Warning,
            UnauthorizedAccessException => LogLevel.Warning,
            _ => LogLevel.Error
        };

        logger.Log(
            logLevel,
            exception,
            "Error [{ErrorCode}] in request {RequestId}: {Message}",
            errorCode,
            requestId,
            exception.Message);
    }
}

public class ErrorResponse
{
    public ApiErrorDto Error { get; set; } = new();
    public int StatusCode { get; set; }
}
