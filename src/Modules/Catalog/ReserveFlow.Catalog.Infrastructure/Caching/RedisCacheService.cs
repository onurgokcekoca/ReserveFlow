using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ReserveFlow.Catalog.Application.Abstractions.Caching;

namespace ReserveFlow.Catalog.Infrastructure.Caching;

public sealed partial class RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            byte[]? data = await cache.GetAsync(key, cancellationToken);

            if (data is null)
                return default;

            return JsonSerializer.Deserialize<T>(data, JsonOptions);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogCacheReadFailure(logger, ex, key);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            byte[] data = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };

            await cache.SetAsync(
                key,
                data,
                options,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogCacheWriteFailure(logger, ex, key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await cache.RemoveAsync(key, cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogCacheInvalidationFailure(logger, ex, key);
        }
    }

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Cache read failed for {Key}")]
    private static partial void LogCacheReadFailure(
        ILogger logger,
        Exception exception,
        string key);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Cache write failed for {Key}")]
    private static partial void LogCacheWriteFailure(
        ILogger logger,
        Exception exception,
        string key);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "Cache invalidation failed for {Key}")]
    private static partial void LogCacheInvalidationFailure(
        ILogger logger,
        Exception exception,
        string key);
}
