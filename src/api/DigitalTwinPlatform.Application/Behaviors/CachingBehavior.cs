using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.Behaviors;

/// <summary>
/// Pipeline behavior for caching query results.
/// Automatically caches query responses and returns cached results on subsequent requests.
/// </summary>
public class CachingBehavior<TRequest, TResponse>(
    IMemoryCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICacheableQuery
{
    private const int DefaultCacheDurationMinutes = 5;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cacheKey = GenerateCacheKey(request);
        
        if (cache.TryGetValue(cacheKey, out TResponse? cachedResult))
        {
            logger.LogInformation("Cache hit for query: {QueryName} with key: {CacheKey}", typeof(TRequest).Name, cacheKey);
            return cachedResult!;
        }

        logger.LogInformation("Cache miss for query: {QueryName} with key: {CacheKey}", typeof(TRequest).Name, cacheKey);
        var result = await next();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(request.CacheDurationMinutes ?? DefaultCacheDurationMinutes));

        cache.Set(cacheKey, result, cacheOptions);
        logger.LogInformation("Cached query result: {QueryName} for {Minutes} minutes", typeof(TRequest).Name, request.CacheDurationMinutes ?? DefaultCacheDurationMinutes);

        return result;
    }

    private static string GenerateCacheKey(TRequest request)
    {
        var requestType = typeof(TRequest).Name;
        var properties = typeof(TRequest).GetProperties();
        var propertyValues = string.Join("_", properties.Select(p => $"{p.Name}={p.GetValue(request)}"));
        return $"{requestType}_{propertyValues}";
    }
}

/// <summary>
/// Marker interface for queries that should be cached.
/// </summary>
public interface ICacheableQuery
{
    int? CacheDurationMinutes { get; }
}
