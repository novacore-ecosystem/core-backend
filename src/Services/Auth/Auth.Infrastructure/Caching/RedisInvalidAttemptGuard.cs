using NovaCore.Auth.Application.Abstractions.Services;

using StackExchange.Redis;

namespace NovaCore.Auth.Infrastructure.Caching;

/// <summary>
/// Redis-backed <see cref="IInvalidAttemptGuard"/>. The failure counter is a plain INCR (atomic
/// and race-free across concurrent requests/instances by construction - Redis serializes
/// commands, so only one caller's increment can ever land on the triggering count), TTL'd to the
/// lock duration so an abandoned attempt sequence eventually clears on its own. Crossing the
/// threshold swaps the counter for a separate lock key whose own TTL is the remaining lock time.
/// </summary>
public sealed class RedisInvalidAttemptGuard(IConnectionMultiplexer connectionMultiplexer) : IInvalidAttemptGuard, IAppService
{
    private const string CounterKeyPrefix = "auth:invalid-attempt-count:";
    private const string LockKeyPrefix = "auth:invalid-attempt-lock:";

    public async Task<AttemptLockStatus> CheckLockAsync(string key, CancellationToken ct = default)
    {
        var db = connectionMultiplexer.GetDatabase();
        var ttl = await db.KeyTimeToLiveAsync($"{LockKeyPrefix}{key}");

        return ttl.HasValue
            ? new AttemptLockStatus(true, Math.Max((int)Math.Ceiling(ttl.Value.TotalSeconds), 1))
            : new AttemptLockStatus(false, 0);
    }

    public async Task<AttemptLockStatus> RecordFailureAsync(string key, int maxAttempts, TimeSpan lockDuration, CancellationToken ct = default)
    {
        var db = connectionMultiplexer.GetDatabase();
        var counterKey = $"{CounterKeyPrefix}{key}";

        var count = await db.StringIncrementAsync(counterKey);
        if (count == 1)
            await db.KeyExpireAsync(counterKey, lockDuration);

        if (count < maxAttempts)
            return new AttemptLockStatus(false, 0);

        await db.StringSetAsync($"{LockKeyPrefix}{key}", 1, lockDuration);
        await db.KeyDeleteAsync(counterKey);

        return new AttemptLockStatus(true, (int)lockDuration.TotalSeconds);
    }

    public Task ResetAsync(string key, CancellationToken ct = default)
    {
        var db = connectionMultiplexer.GetDatabase();
        return Task.WhenAll(
            db.KeyDeleteAsync($"{CounterKeyPrefix}{key}"),
            db.KeyDeleteAsync($"{LockKeyPrefix}{key}"));
    }
}
