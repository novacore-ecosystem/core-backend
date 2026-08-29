namespace NovaCore.BuildingBlock.Application.Abstractions.Caching;

/// <summary>
/// Generic three-layer (local memory -> Redis -> authoritative source) versioned cache. Deliberately
/// not tied to any single service's DTO: <c>TValue</c> and the caller-supplied <paramref
/// name="refreshFactory"/> below are the only domain-specific inputs, so the same implementation
/// backs Auth's effective-permission cache today and any other permission-shaped read-heavy data
/// tomorrow.
///
/// Per the project's mandatory "no cache decorators" rule (docs/reference/caching.md), this is not,
/// and must never become, a transparent wrapper around a Persistence-facing interface - callers
/// inject it explicitly and call it directly, at the point in their workflow where a cached read
/// (vs. an authoritative one) is actually wanted.
/// </summary>
public interface IVersionedCache<TValue>
{
    /// <summary>Local memory hit returns immediately. Otherwise tries Redis, and only on a full miss
    /// invokes <paramref name="refreshFactory"/> (the authoritative source) - with stampede
    /// protection so concurrent misses on the same key coalesce into one call. A null result from
    /// <paramref name="refreshFactory"/> is returned as-is and not cached.</summary>
    Task<VersionedCacheEntry<TValue>?> GetOrRefreshAsync(
        string key,
        Func<CancellationToken, Task<VersionedCacheEntry<TValue>?>> refreshFactory,
        CancellationToken ct = default);

    /// <summary>Removes the key from both local memory and Redis, then publishes a cache-change
    /// signal so other instances evict their own local copy. <paramref name="newVersion"/> is
    /// null for a true delete (entity gone); non-null for "changed to this version" (still removes
    /// the entry - the next read re-populates it).</summary>
    Task InvalidateAsync(string key, long? newVersion = null, CancellationToken ct = default);
}
