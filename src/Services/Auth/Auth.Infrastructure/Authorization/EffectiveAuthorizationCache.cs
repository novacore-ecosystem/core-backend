using Microsoft.Extensions.Configuration;

using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.BuildingBlock.Application.Exceptions;

namespace NovaCore.Auth.Infrastructure.Authorization;

/// <summary>
/// Read-through cache for <see cref="AccountAuthorizationSnapshot"/>.
/// </summary>
/// <remarks>
/// On a miss, resolves Level (<see cref="IAccountReadService"/>) and the effective permission set
/// (<see cref="IEffectivePermissionReadService"/>) and caches the combined snapshot - one Redis
/// round trip serves every backend authorization check instead of each re-running the
/// Role/Position/PermissionGrant join. Explicit cache called directly by
/// <c>AccountAuthorizationService</c>, not a decorator (see docs/reference/caching.md).
/// </remarks>
public sealed class EffectiveAuthorizationCache(
    IAccountReadService accountReadService,
    IEffectivePermissionReadService effectivePermissionReadService,
    ICacheService cacheService,
    IConfiguration configuration) : IEffectiveAuthorizationCache
{
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(
        configuration
            .GetSection("Caching:EntityTtl:AccountAuthorization:MinutesToExpire")
            .Get<int?>() ?? CacheKeyConstant.AccountAuthorization.DefaultTtlMinutes);

    public async Task<AccountAuthorizationSnapshot> GetAsync(
        Guid accountId,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var key = CacheKeyConstant.AccountAuthorization.Snapshot(tenantId, accountId);

        var cached = await cacheService.GetAsync<AccountAuthorizationSnapshot>(key, ct);
        if (cached is not null)
            return cached;

        var account = await accountReadService.GetByIdAsync(accountId, ct)
            ?? throw new NotFoundException("Account", accountId);
        var permissionKeys = await effectivePermissionReadService.GetEffectivePermissionsAsync(accountId, tenantId, ct);

        var snapshot = new AccountAuthorizationSnapshot(account.Level, permissionKeys);
        await cacheService.SetAsync(key, snapshot, _defaultTtl, ct);

        return snapshot;
    }

    public Task InvalidateAsync(
        Guid accountId,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var key = CacheKeyConstant.AccountAuthorization.Snapshot(tenantId, accountId);
        return cacheService.RemoveAsync(key, ct);
    }
}
