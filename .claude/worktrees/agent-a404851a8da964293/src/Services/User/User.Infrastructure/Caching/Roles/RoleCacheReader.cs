using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using NovaCore.User.Application.Abstractions.Services;

namespace NovaCore.User.Infrastructure.Caching.Roles;

/// <summary>
/// Reads roles from the shared Redis cache under the same key Auth's
/// RoleCacheService writes to (CacheKeys.Roles.UserRoles). User service never
/// writes to this key - Auth owns population/invalidation. A miss just means
/// "nothing cached right now" (e.g. TTL expired, or the user hasn't triggered
/// a role check in Auth recently), not an error - callers get an empty list.
/// </summary>
public sealed class RoleCacheReader(
    ICacheService cacheService,
    IAuthClientService authClientService) : IRoleCacheReader
{
    public async Task<IReadOnlyList<string>> GetUserRolesAsync(Guid userId, CancellationToken ct = default)
    {
        var key = CacheKeyConstant.Roles.UserRoles(userId);
        var cached = await cacheService.GetAsync<List<string>>(key, ct);

        if (cached is null)
        {
            var roles = await authClientService.GetUserRolesAsync(userId, ct);
            return [.. roles];
        }

        return cached;
    }
}
