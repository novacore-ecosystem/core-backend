using NovaCore.BuildingBlock.Infrastructure.Caching;

using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Infrastructure.Extensions;

/// <summary>
/// Named convenience wiring the generic versioned-cache infrastructure for the permission-shaped
/// value every service needs first: an account's effective permission keys. Other permission-shaped
/// caches (definitions, localized catalogs, ...) register their own <c>TValue</c> directly via
/// <see cref="VersionedCachingExtensions.AddVersionedCache{TValue}"/> - this method only saves the
/// boilerplate for the common case.
/// </summary>
public static class PermissionCachingExtensions
{
    /// <summary>Requires <c>AddRedisCache</c> to already be registered. <paramref name="channelName"/>
    /// is owned by the calling service (e.g. Auth's own <c>"authorization.permission.changed"</c>
    /// constant) - this method stays domain-agnostic so other services can reuse it under their own
    /// channel name without colliding.</summary>
    public static IServiceCollection AddPermissionCaching(
        this IServiceCollection services,
        string channelName,
        Action<VersionedCacheOptions<IReadOnlySet<string>>>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(channelName);

        services.AddVersionedCache("Permissions", channelName, configure);
        services.AddRedisCacheChangePubSub();

        return services;
    }
}
