using NovaCore.BuildingBlock.Application.Abstractions.Idempotency;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace NovaCore.BuildingBlock.Infrastructure.Idempotency;

/// <summary>Redis-backed <see cref="IDistributedLockProvider"/> using SET NX PX + poll, with a token-guarded release.</summary>
internal sealed class RedisDistributedLockProvider(
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<RedisDistributedLockProvider> logger) : IDistributedLockProvider
{
    private const string KeyPrefix = "lock:";
    private static readonly TimeSpan PollDelay = TimeSpan.FromMilliseconds(100);

    public async Task<IDistributedLock?> AcquireAsync(
        string resource,
        TimeSpan expiration,
        TimeSpan timeout,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);

        var redisKey = $"{KeyPrefix}{resource}";
        var token = Guid.NewGuid().ToString("N");
        var db = connectionMultiplexer.GetDatabase();
        var deadline = DateTime.UtcNow + timeout;

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            var acquired = await db.StringSetAsync(redisKey, token, expiration, When.NotExists);
            if (acquired)
            {
                logger.LogInformation("Lock acquired for resource {Resource}", resource);
                return new RedisDistributedLock(connectionMultiplexer, redisKey, resource, token, logger);
            }

            if (DateTime.UtcNow >= deadline)
            {
                logger.LogWarning("Lock timeout waiting for resource {Resource} after {TimeoutSeconds}s", resource, timeout.TotalSeconds);
                return null;
            }

            await Task.Delay(PollDelay, ct);
        }
    }
}
