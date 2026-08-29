using NovaCore.BuildingBlock.Application.Abstractions.Caching;
using NovaCore.BuildingBlock.Infrastructure.Caching;
using NovaCore.BuildingBlock.Infrastructure.PubSub;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace NovaCore.BuildingBlock.Infrastructure.Extensions;

/// <summary>
/// DI registration for the generic three-layer permission-shaped cache. A consuming service calls
/// <see cref="AddVersionedCache{TValue}"/> once per cached value type, then injects
/// <see cref="IVersionedCache{TValue}"/>; <see cref="AddRedisCacheChangePubSub"/> is a separate,
/// optional call that upgrades cross-instance synchronization from a no-op to real Redis Pub/Sub -
/// call order between the two does not matter.
/// </summary>
public static class VersionedCachingExtensions
{
    /// <summary>Registers a singleton <see cref="IVersionedCache{TValue}"/> (local memory + Redis,
    /// via the existing <c>ICacheService</c>/<c>IConnectionMultiplexer</c>). Requires
    /// <c>AddRedisCache</c> to already be registered. Defaults cross-instance synchronization to a
    /// no-op publisher - call <see cref="AddRedisCacheChangePubSub"/> as well to enable it.</summary>
    public static IServiceCollection AddVersionedCache<TValue>(
        this IServiceCollection services,
        string cacheName,
        string channelName,
        Action<VersionedCacheOptions<TValue>>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheName);
        ArgumentException.ThrowIfNullOrWhiteSpace(channelName);

        services.AddMemoryCache();
        services.TryAddSingleton<ICacheChangePublisher, NullCacheChangePublisher>();

        services.Configure<VersionedCacheOptions<TValue>>(o =>
        {
            o.CacheName = cacheName;
            o.ChannelName = channelName;
            configure?.Invoke(o);
        });

        services.AddSingleton<LayeredVersionedCache<TValue>>();
        services.AddSingleton<IVersionedCache<TValue>>(sp => sp.GetRequiredService<LayeredVersionedCache<TValue>>());
        services.AddSingleton<ICacheChangeListener>(sp => sp.GetRequiredService<LayeredVersionedCache<TValue>>());

        return services;
    }

    /// <summary>Upgrades cache-change synchronization to real Redis Pub/Sub: supersedes the no-op
    /// <see cref="ICacheChangePublisher"/> default and starts one shared subscriber hosted service
    /// that dispatches to every registered <see cref="ICacheChangeListener"/> (i.e. every
    /// <see cref="AddVersionedCache{TValue}"/>-registered cache), regardless of how many distinct
    /// <c>TValue</c> caches a service has registered. Safe to call more than once.</summary>
    public static IServiceCollection AddRedisCacheChangePubSub(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ICacheChangePublisher, RedisCacheChangePublisher>();

        if (!services.Any(sd => sd.ImplementationType == typeof(RedisCacheChangeSubscriberHostedService)))
            services.AddHostedService<RedisCacheChangeSubscriberHostedService>();

        return services;
    }
}
