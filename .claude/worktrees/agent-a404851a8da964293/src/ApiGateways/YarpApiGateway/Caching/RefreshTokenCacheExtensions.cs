using NovaCore.BuildingBlock.SharedKernel.Constants;

using StackExchange.Redis;

namespace NovaCore.YarpApiGateway.Caching;

/// <summary>
/// Minimal, read-only Redis lookup for refresh token existence. Local to the Gateway since it's
/// the only consumer - a single EXISTS check doesn't warrant pulling in a shared caching abstraction.
/// </summary>
public static class RefreshTokenCacheExtensions
{
    public static IServiceCollection AddRefreshTokenCache(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connectionString));

        return services;
    }

    public static async Task<bool> RefreshTokenExistsAsync(
        this IConnectionMultiplexer redis,
        string refreshToken,
        CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        return await db.KeyExistsAsync(CacheKeyConstant.RefreshTokens.ByTokenString(refreshToken));
    }
}
