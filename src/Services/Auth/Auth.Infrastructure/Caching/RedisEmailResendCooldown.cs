using NovaCore.Auth.Application.Abstractions.Services;

using StackExchange.Redis;

namespace NovaCore.Auth.Infrastructure.Caching;

/// <summary>
/// Redis SET NX-backed <see cref="IEmailResendCooldown"/>
/// the same atomic primitive <see cref="Idempotency.RedisDistributedLockProvider"/> uses, without a release-on-dispose since a cooldown window must survive past the request that claimed it.
/// </summary>
public sealed class RedisEmailResendCooldown(IConnectionMultiplexer connectionMultiplexer) : IEmailResendCooldown, IAppService
{
    private const string KeyPrefix = "auth:email-resend-cooldown:";

    public async Task<CooldownClaim> TryClaimAsync(string key, TimeSpan window, CancellationToken ct = default)
    {
        var redisKey = $"{KeyPrefix}{key}";
        var db = connectionMultiplexer.GetDatabase();

        var claimed = await db.StringSetAsync(redisKey, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), window, When.NotExists);
        if (claimed)
            return new CooldownClaim(true, 0);

        var ttl = await db.KeyTimeToLiveAsync(redisKey);
        var remainingSeconds = ttl.HasValue ? (int)Math.Ceiling(ttl.Value.TotalSeconds) : 0;

        return new CooldownClaim(false, Math.Max(remainingSeconds, 1));
    }

    public Task ReleaseAsync(string key, CancellationToken ct = default) =>
        connectionMultiplexer.GetDatabase().KeyDeleteAsync($"{KeyPrefix}{key}");
}
