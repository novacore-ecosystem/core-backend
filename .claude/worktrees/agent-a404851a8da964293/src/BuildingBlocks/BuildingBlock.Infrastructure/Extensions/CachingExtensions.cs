using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Infrastructure.Caching;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

namespace NovaCore.BuildingBlock.Infrastructure.Extensions;

/// <summary>
/// Caching Extensions
/// </summary>
public static class CachingExtensions
{
    /// <summary>Add Redis cache from appsettings.json "Cache" section</summary>
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new CacheOptions();
        configuration.GetSection(CacheOptions.Section).Bind(options);
        return services.AddRedisCache(options);
    }

    /// <summary>Add Redis cache with explicit CacheOptions</summary>
    public static IServiceCollection AddRedisCache(this IServiceCollection services, CacheOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new InvalidOperationException(
                $"Redis connection string required. Set 'Cache:ConnectionString' in appsettings.json");

        services.AddSingleton(options);

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisOptions = ConfigurationOptions.Parse(options.ConnectionString);
            redisOptions.ConnectTimeout = options.ConnectionTimeout;
            redisOptions.SyncTimeout = options.ConnectionTimeout;
            return ConnectionMultiplexer.Connect(redisOptions);
        });

        services.AddSingleton<ICacheService, RedisCacheService>();
        return services;
    }

    /// <summary>Add Redis cache with connection string only</summary>
    public static IServiceCollection AddRedisCache(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        return services.AddRedisCache(new CacheOptions { ConnectionString = connectionString });
    }
}
