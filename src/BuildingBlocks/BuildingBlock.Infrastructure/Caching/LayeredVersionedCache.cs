using System.Collections.Concurrent;

using NovaCore.BuildingBlock.Application.Abstractions.Caching;
using NovaCore.BuildingBlock.Application.Abstractions.Idempotency;
using NovaCore.BuildingBlock.Application.Abstractions.Services;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NovaCore.BuildingBlock.Infrastructure.Caching;

/// <summary>
/// Three-layer (local memory -> Redis -> caller-supplied authoritative source) implementation of
/// <see cref="IVersionedCache{TValue}"/>. Registered as a singleton per <c>TValue</c> via
/// <c>AddVersionedCache</c>, exposed as both <see cref="IVersionedCache{TValue}"/> (for callers) and
/// <see cref="ICacheChangeListener"/> (so the Pub/Sub subscriber can refresh its local memory when
/// another instance invalidates the same key).
///
/// Redis is never a single point of failure: an L2 read/write/removal failure is caught, logged, and
/// treated as a miss/no-op rather than thrown, and stampede protection degrades to in-process-only
/// coordination when the distributed lock is unavailable. See docs/reference/caching.md's
/// "no decorators" rule - this stays a plain, explicitly-injected service, not a wrapper around any
/// Persistence-facing interface.
/// </summary>
internal sealed class LayeredVersionedCache<TValue>(
    IMemoryCache memoryCache,
    ICacheService cacheService,
    IDistributedLockProvider lockProvider,
    ICacheChangePublisher publisher,
    IOptions<VersionedCacheOptions<TValue>> options,
    ILogger<LayeredVersionedCache<TValue>> logger) : IVersionedCache<TValue>, ICacheChangeListener
{
    private const string LockResourcePrefix = "permcache-refresh:";

    private readonly VersionedCacheOptions<TValue> _options = options.Value;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _gates = new(StringComparer.Ordinal);

    public string Channel => _options.ChannelName;

    public async Task<VersionedCacheEntry<TValue>?> GetOrRefreshAsync(
        string key,
        Func<CancellationToken, Task<VersionedCacheEntry<TValue>?>> refreshFactory,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(refreshFactory);

        if (TryGetL1(key, out var l1))
            return l1;

        var gate = _gates.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            if (TryGetL1(key, out l1))
                return l1;

            var l2 = await TryGetL2Async(key, ct);
            if (l2 is not null)
            {
                SetL1(key, l2);
                return l2;
            }

            await using var distLock = await TryAcquireLockAsync(key, ct);
            if (distLock is null)
            {
                // Either contended (another instance is already refreshing) or Redis is down - a
                // courtesy re-check before falling through, but never block waiting for the holder.
                l2 = await TryGetL2Async(key, ct);
                if (l2 is not null)
                {
                    SetL1(key, l2);
                    return l2;
                }
            }

            var refreshed = await refreshFactory(ct);
            if (refreshed is null)
                return null;

            await TrySetL2Async(key, refreshed, ct);
            SetL1(key, refreshed);
            return refreshed;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task InvalidateAsync(string key, long? newVersion = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        memoryCache.Remove(key);

        try
        {
            await cacheService.RemoveAsync(key, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis removal failed for {CacheName} key {Key} - relying on TTL until the next miss", _options.CacheName, key);
        }

        try
        {
            await publisher.PublishAsync(
                _options.ChannelName,
                new CacheChangeMessage(key, newVersion ?? 0, CacheChangeOperation.Invalidate, DateTimeOffset.UtcNow),
                ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache-change publish failed for {CacheName} key {Key} - other instances rely on TTL until their next miss", _options.CacheName, key);
        }
    }

    public void OnCacheChange(CacheChangeMessage message)
    {
        if (message.Operation == CacheChangeOperation.Invalidate)
        {
            memoryCache.Remove(message.Key);
            return;
        }

        if (TryGetL1(message.Key, out var cached) && message.Version > cached!.Version)
            memoryCache.Remove(message.Key);
    }

    private bool TryGetL1(string key, out VersionedCacheEntry<TValue>? entry)
        => memoryCache.TryGetValue(key, out entry) && entry is not null;

    private void SetL1(string key, VersionedCacheEntry<TValue> entry)
        => memoryCache.Set(key, entry, _options.LocalMemoryTtl);

    private async Task<VersionedCacheEntry<TValue>?> TryGetL2Async(string key, CancellationToken ct)
    {
        try
        {
            return await cacheService.GetAsync<VersionedCacheEntry<TValue>>(key, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis read failed for {CacheName} key {Key} - treating as a miss", _options.CacheName, key);
            return null;
        }
    }

    private async Task TrySetL2Async(string key, VersionedCacheEntry<TValue> value, CancellationToken ct)
    {
        try
        {
            await cacheService.SetAsync(key, value, _options.RedisTtl, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis write failed for {CacheName} key {Key}", _options.CacheName, key);
        }
    }

    private async Task<IDistributedLock?> TryAcquireLockAsync(string key, CancellationToken ct)
    {
        try
        {
            return await lockProvider.AcquireAsync($"{LockResourcePrefix}{key}", _options.LockExpiration, _options.LockTimeout, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Distributed lock unavailable for {CacheName} key {Key} - proceeding without cross-instance coordination", _options.CacheName, key);
            return null;
        }
    }
}
