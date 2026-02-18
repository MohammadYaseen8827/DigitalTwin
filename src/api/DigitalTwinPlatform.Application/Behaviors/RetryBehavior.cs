using MediatR;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.Behaviors;

/// <summary>
/// Pipeline behavior for retrying failed requests with exponential backoff.
/// Automatically retries transient failures (network issues, timeouts, etc.).
/// </summary>
public class RetryBehavior<TRequest, TResponse>(ILogger<RetryBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int MaxRetries = 3;
    private const int InitialDelayMs = 100;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        int retryCount = 0;

        while (true)
        {
            try
            {
                if (retryCount > 0)
                {
                    logger.LogInformation(
                        "Retrying request {RequestName} (attempt {RetryCount}/{MaxRetries})",
                        requestName,
                        retryCount + 1,
                        MaxRetries);
                }

                return await next();
            }
            catch (Exception ex) when (IsTransientError(ex) && retryCount < MaxRetries)
            {
                retryCount++;
                var delayMs = CalculateExponentialBackoff(retryCount);

                logger.LogWarning(
                    ex,
                    "Transient error in request {RequestName}. Retrying in {DelayMs}ms (attempt {RetryCount}/{MaxRetries})",
                    requestName,
                    delayMs,
                    retryCount,
                    MaxRetries);

                await Task.Delay(delayMs, cancellationToken);
            }
            catch (Exception ex) when (!IsTransientError(ex))
            {
                logger.LogError(ex, "Non-transient error in request {RequestName}. Not retrying.", requestName);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Request {RequestName} failed after {RetryCount} retries",
                    requestName,
                    retryCount);
                throw;
            }
        }
    }

    private static bool IsTransientError(Exception ex)
    {
        // Transient errors that should be retried
        return ex is TimeoutException ||
               ex is HttpRequestException ||
               ex is InvalidOperationException && ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
               ex is IOException ||
               (ex.InnerException != null && IsTransientError(ex.InnerException));
    }

    private static int CalculateExponentialBackoff(int retryCount)
    {
        // Exponential backoff: 100ms, 200ms, 400ms
        return InitialDelayMs * (int)Math.Pow(2, retryCount - 1);
    }
}
