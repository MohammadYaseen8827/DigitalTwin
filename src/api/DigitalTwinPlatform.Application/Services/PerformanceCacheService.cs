using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace DigitalTwinPlatform.Application.Services
{
    /// <summary>
    /// High-performance caching service with multiple caching layers
    /// </summary>
    public interface IPerformanceCacheService
    {
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration);
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
        void Clear();
    }

    public class PerformanceCacheService : IPerformanceCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<PerformanceCacheService> _logger;
        private readonly ConcurrentDictionary<string, DateTimeOffset> _cacheKeys;

        public PerformanceCacheService(
            IMemoryCache memoryCache,
            ILogger<PerformanceCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _cacheKeys = new ConcurrentDictionary<string, DateTimeOffset>();
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration)
        {
            // Try to get from memory cache first
            if (_memoryCache.TryGetValue(key, out T? cachedValue) && cachedValue != null)
            {
                _logger.LogDebug("Cache HIT for key: {Key}", key);
                return cachedValue;
            }

            _logger.LogDebug("Cache MISS for key: {Key}, fetching from source", key);

            // Fetch from source
            var value = await factory();
            
            // Cache the result
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration)
                .RegisterPostEvictionCallback(EvictionCallback);

            _memoryCache.Set(key, value, cacheEntryOptions);
            _cacheKeys.TryAdd(key, DateTimeOffset.UtcNow);

            return value;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            return await Task.FromResult(_memoryCache.TryGetValue<T>(key, out var value) ? value : default);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration)
                .RegisterPostEvictionCallback(EvictionCallback);

            _memoryCache.Set(key, value, cacheEntryOptions);
            _cacheKeys.TryAdd(key, DateTimeOffset.UtcNow);

            await Task.CompletedTask;
        }

        public async Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
            await Task.CompletedTask;
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            var keysToRemove = _cacheKeys.Keys.Where(k => k.Contains(pattern)).ToList();
            
            foreach (var key in keysToRemove)
            {
                await RemoveAsync(key);
            }
        }

        public void Clear()
        {
            // Remove all tracked cache entries
            var keysToRemove = _cacheKeys.Keys.ToList();
            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _cacheKeys.TryRemove(key, out _);
            }
        }

        private void EvictionCallback(object key, object value, EvictionReason reason, object state)
        {
            if (key is string cacheKey)
            {
                _cacheKeys.TryRemove(cacheKey, out _);
                _logger.LogDebug("Cache entry evicted: {Key} - Reason: {Reason}", cacheKey, reason);
            }
        }
    }
}