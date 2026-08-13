using Microsoft.Extensions.Configuration;

using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

namespace NovaCore.Auth.Infrastructure.Caching;

/// <summary>
/// Read-through cache for a tenant's bootstrap Version - the fast path a future Notification Hub
/// connection handler is expected to read from (see docs/services/auth-service.md, "Redis Version
/// Cache"). A miss falls back to the database (the source of truth) and refreshes Redis; there is
/// no independent write path - InvalidateAsync just clears the cache and lets the next read
/// refresh it, rather than trying to keep a cached copy of a value it doesn't own.
/// </summary>
public interface ITenantVersionCache
{
    /// <summary>Null only when the tenant genuinely doesn't exist or is inactive - callers treat
    /// that the same way they would a cache-unrelated "no tenant" result.</summary>
    Task<int?> GetOrRefreshAsync(Guid tenantId, CancellationToken ct = default);

    Task InvalidateAsync(Guid tenantId, CancellationToken ct = default);
}

public sealed class TenantVersionCache(
    ICacheService cacheService,
    ITenantReadService tenantReadService,
    IConfiguration configuration) : ITenantVersionCache
{
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(
        configuration
            .GetSection("Caching:EntityTtl:Tenants:MinutesToExpire")
            .Get<int?>() ?? CacheKeyConstant.Tenants.DefaultTtlMinutes);

    public async Task<int?> GetOrRefreshAsync(Guid tenantId, CancellationToken ct = default)
    {
        var key = CacheKeyConstant.Tenants.Version(tenantId);

        var cached = await cacheService.GetAsync<int?>(key, ct);
        if (cached is not null)
            return cached;

        var current = await tenantReadService.GetVersionAsync(tenantId, ct);
        if (current is null || !current.Value.IsActive)
            return null;

        await cacheService.SetAsync(key, current.Value.Version, _defaultTtl, ct);

        return current.Value.Version;
    }

    public async Task InvalidateAsync(Guid tenantId, CancellationToken ct = default)
    {
        await cacheService.RemoveAsync(CacheKeyConstant.Tenants.Version(tenantId), ct);
    }
}
