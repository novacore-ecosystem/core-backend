using Microsoft.Extensions.Configuration;

using NovaCore.Auth.Application.Abstractions.Apps;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;

namespace NovaCore.Auth.Infrastructure.Caching.Apps;

/// <summary>
/// Read-through cache of each App's AccountID membership set
/// (<see cref="CacheKeyConstant.Apps.AccountIds"/>), keyed per App - one Redis round trip serves
/// a membership check instead of a per-request database query.
/// </summary>
/// <remarks>
/// On a miss, loads every AccountID assigned to the App in one query
/// (<see cref="IAccountAppAssignmentService.GetAccountIdsByAppAsync"/>), builds the membership
/// set, and caches it. Explicit cache called directly by Login/RefreshToken, not a decorator
/// (see docs/reference/caching.md).
/// </remarks>
public sealed class AppMembershipCache(
    IAccountAppAssignmentService accountAppAssignmentService,
    ICacheService cacheService,
    IConfiguration configuration) : IAppMembershipCache
{
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(
        configuration
            .GetSection("Caching:EntityTtl:AppMembership:MinutesToExpire")
            .Get<int?>() ?? CacheKeyConstant.Apps.AccountIdsDefaultTtlMinutes);

    public async Task<bool> IsAssignedAsync(Guid appId, Guid accountId, CancellationToken ct = default)
    {
        var key = CacheKeyConstant.Apps.AccountIds(appId);

        var cached = await cacheService.GetAsync<HashSet<Guid>>(key, ct);
        if (cached is not null)
            return cached.Contains(accountId);

        var accountIds = await accountAppAssignmentService.GetAccountIdsByAppAsync(appId, ct);
        var membershipSet = new HashSet<Guid>(accountIds);

        await cacheService.SetAsync(key, membershipSet, _defaultTtl, ct);

        return membershipSet.Contains(accountId);
    }

    public Task InvalidateAsync(Guid appId, CancellationToken ct = default)
    {
        return cacheService.RemoveAsync(CacheKeyConstant.Apps.AccountIds(appId), ct);
    }
}
