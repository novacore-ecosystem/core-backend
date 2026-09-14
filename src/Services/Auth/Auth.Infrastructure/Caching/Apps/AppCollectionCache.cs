using Microsoft.Extensions.Configuration;

using NovaCore.Auth.Application.Abstractions.Apps;
using NovaCore.Auth.Application.Abstractions.Persistence.Apps;
using NovaCore.Auth.Domain.ValueObjects;

namespace NovaCore.Auth.Infrastructure.Caching.Apps;

/// <summary>
/// Read-through cache for the entire App collection under one Redis key
/// (<see cref="CacheKeyConstant.Apps.Collection"/>) - the number of Apps is expected to stay
/// small, so caching one array is simpler and cheaper than a per-App entry.
/// </summary>
/// <remarks>
/// On a miss, loads every App in one query (<see cref="IAppReadService.GetAllAsync"/>) and caches
/// the lightweight projection. Explicit cache called directly by Login/Register/RefreshToken, not
/// a decorator (see docs/reference/caching.md).
/// </remarks>
public sealed class AppCollectionCache(
    IAppReadService appReadService,
    ICacheService cacheService,
    IConfiguration configuration) : IAppCollectionCache
{
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(
        configuration
            .GetSection("Caching:EntityTtl:Apps:MinutesToExpire")
            .Get<int?>() ?? CacheKeyConstant.Apps.DefaultTtlMinutes);

    public async Task<IReadOnlyList<CachedApp>> GetAllAsync(CancellationToken ct = default)
    {
        var cached = await cacheService.GetAsync<List<CachedApp>>(CacheKeyConstant.Apps.Collection, ct);
        if (cached is not null)
            return cached;

        var apps = await appReadService.GetAllAsync(ct);
        var snapshot = apps
            .Select(a => new CachedApp(a.Id, a.Code.Value, a.Name, a.IsActive))
            .ToList();

        await cacheService.SetAsync(CacheKeyConstant.Apps.Collection, snapshot, _defaultTtl, ct);

        return snapshot;
    }

    public async Task<CachedApp?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var normalizedCode = AppCode.Create(code).Value;
        var apps = await GetAllAsync(ct);

        return apps.FirstOrDefault(a => a.Code == normalizedCode);
    }

    public Task InvalidateAsync(CancellationToken ct = default)
    {
        return cacheService.RemoveAsync(CacheKeyConstant.Apps.Collection, ct);
    }
}
