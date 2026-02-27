using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace DigitalTwinPlatform.API.Filters;

/// <summary>
/// Global exception filter to handle unhandled exceptions and return safe error responses.
/// Prevents implementation detail leakage (e.g., stack traces, internal error messages) to clients.
/// </summary>
public class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        var traceId = context.HttpContext.TraceIdentifier;

        logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", traceId);

        var result = new ObjectResult(new
        {
            Error = "An unexpected error occurred. Please contact support.",
            Message = "Internal Server Error",
            TraceId = traceId
        })
        {
            StatusCode = (int)HttpStatusCode.InternalServerError
        };

        context.Result = result;
        context.ExceptionHandled = true;
    }
}
