using Microsoft.Extensions.Configuration;

using NovaCore.Auth.Application.Abstractions.Persistence.Registrations;
using NovaCore.Auth.Application.Abstractions.Registrations;

namespace NovaCore.Auth.Infrastructure.Caching.Registrations;

/// <summary>
/// Read-through cache for <see cref="RegistrationDefaultsSnapshot"/>, one entry per (Tenant, App).
/// </summary>
/// <remarks>
/// On a miss, resolves the default Role ids and permission keys
/// (<see cref="IRegistrationDefaultsReadService"/>) and caches the combined snapshot. Explicit
/// cache called directly by <c>RegisterHandler</c>, not a decorator (see docs/reference/caching.md).
/// </remarks>
public sealed class RegistrationDefaultsCache(
    IRegistrationDefaultsReadService registrationDefaultsReadService,
    ICacheService cacheService,
    IConfiguration configuration) : IRegistrationDefaultsCache
{
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(
        configuration
            .GetSection("Caching:EntityTtl:RegistrationDefaults:MinutesToExpire")
            .Get<int?>() ?? CacheKeyConstant.RegistrationDefaults.DefaultTtlMinutes);

    public async Task<RegistrationDefaultsSnapshot> GetAsync(Guid tenantId, Guid appId, CancellationToken ct = default)
    {
        var key = CacheKeyConstant.RegistrationDefaults.Snapshot(tenantId, appId);

        var cached = await cacheService.GetAsync<RegistrationDefaultsSnapshot>(key, ct);
        return cached ?? await LoadAndCacheAsync(tenantId, appId, key, ct);
    }

    public Task InvalidateAsync(Guid tenantId, Guid appId, CancellationToken ct = default)
    {
        var key = CacheKeyConstant.RegistrationDefaults.Snapshot(tenantId, appId);
        return cacheService.RemoveAsync(key, ct);
    }

    public async Task RefreshAsync(Guid tenantId, Guid appId, CancellationToken ct = default)
    {
        var key = CacheKeyConstant.RegistrationDefaults.Snapshot(tenantId, appId);
        await cacheService.RemoveAsync(key, ct);
        await LoadAndCacheAsync(tenantId, appId, key, ct);
    }

    private async Task<RegistrationDefaultsSnapshot> LoadAndCacheAsync(
        Guid tenantId, Guid appId, string key, CancellationToken ct)
    {
        var roleIds = await registrationDefaultsReadService.GetDefaultRoleIdsAsync(tenantId, appId, ct);
        var permissionKeys = await registrationDefaultsReadService.GetDefaultPermissionKeysAsync(tenantId, appId, ct);
        var snapshot = new RegistrationDefaultsSnapshot(roleIds, permissionKeys);

        await cacheService.SetAsync(key, snapshot, _defaultTtl, ct);
        return snapshot;
    }
}
