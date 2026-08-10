using Microsoft.Extensions.Configuration;

namespace NovaCore.Auth.Infrastructure.Caching;

/// <summary>
/// Service for managing user role cache operations.
/// Provides centralized cache management for role data with configurable TTL.
/// </summary>
public sealed class RoleCacheService(
    ICacheService cacheService,
    IConfiguration configuration)
{
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(
        configuration
            .GetSection("Caching:EntityTtl:Roles:MinutesToExpire")
            .Get<int?>() ?? CacheKeyConstant.Roles.DefaultTtlMinutes);

    /// <summary>Gets cached user roles. Returns null if not found in cache.</summary>
    public async Task<IList<string>?> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var key = CacheKeyConstant.Roles.UserRoles(userId);
        return await cacheService.GetAsync<List<string>>(key, ct);
    }

    /// <summary>Caches user roles with default TTL from configuration.</summary>
    public async Task SetAsync(Guid userId, IList<string> roles, CancellationToken ct = default)
    {
        var key = CacheKeyConstant.Roles.UserRoles(userId);
        await cacheService.SetAsync(key, new List<string>(roles), _defaultTtl, ct);
    }

    /// <summary>Caches user roles with custom TTL.</summary>
    public async Task SetAsync(Guid userId, IList<string> roles, TimeSpan ttl, CancellationToken ct = default)
    {
        var key = CacheKeyConstant.Roles.UserRoles(userId);
        await cacheService.SetAsync(key, new List<string>(roles), ttl, ct);
    }

    /// <summary>Removes cached roles for a specific user.</summary>
    public async Task RemoveAsync(Guid userId, CancellationToken ct = default)
    {
        var key = CacheKeyConstant.Roles.UserRoles(userId);
        await cacheService.RemoveAsync(key, ct);
    }

    /// <summary>Removes cached roles for multiple users.</summary>
    public async Task RemoveManyAsync(IEnumerable<Guid> userIds, CancellationToken ct = default)
    {
        var keys = userIds.Select(id => CacheKeyConstant.Roles.UserRoles(id)).ToList();
        if (keys.Count > 0)
            await cacheService.RemoveManyAsync(keys, ct);
    }

    /// <summary>Removes all cached role entries matching the pattern.</summary>
    public async Task RemoveByPatternAsync(CancellationToken ct = default)
    {
        await cacheService.RemoveByPatternAsync(CacheKeyConstant.Roles.UserRolesPattern, ct);
    }

    /// <summary>Checks if roles are cached for a user.</summary>
    public async Task<bool> ExistsAsync(Guid userId, CancellationToken ct = default)
    {
        var key = CacheKeyConstant.Roles.UserRoles(userId);
        return await cacheService.ExistsAsync(key, ct);
    }
}
