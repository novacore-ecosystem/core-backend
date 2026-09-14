namespace NovaCore.Auth.Application.Abstractions.Apps;

/// <summary>
/// Read-through cache for the entire App collection, held under a single cache key since the
/// number of Apps is expected to stay small - avoids a per-App cache entry and a per-request
/// database read on App lookup (Login/Register/RefreshToken all resolve an App by Code).
/// </summary>
/// <remarks>
/// Follows the Effective Authorization / User Detail cache pattern (see docs/reference/caching.md)
/// - an explicit Infrastructure service called directly by callers, not a decorator.
/// </remarks>
public interface IAppCollectionCache
{
    /// <summary>Gets every App, populating the cache on a miss.</summary>
    Task<IReadOnlyList<CachedApp>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Resolves one App by its stable Code from the cached collection. Null if no App
    /// with that code exists.</summary>
    /// <param name="code">The App code as received from the client (normalized internally).</param>
    Task<CachedApp?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>Removes the cached App collection, e.g. after an App is created/updated/deleted -
    /// the next read rebuilds it from the database.</summary>
    Task InvalidateAsync(CancellationToken ct = default);
}
