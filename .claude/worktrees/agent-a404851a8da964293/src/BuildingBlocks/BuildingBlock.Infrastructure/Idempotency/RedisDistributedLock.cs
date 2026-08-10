using NovaCore.BuildingBlock.Application.Abstractions.Idempotency;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace NovaCore.BuildingBlock.Infrastructure.Idempotency;

/// <summary>
/// A lock held via a Redis key. Release is a compare-and-delete Lua script keyed on <see cref="_token"/>
/// so this holder can never release a lock some other holder acquired after this one expired.
/// </summary>
internal sealed class RedisDistributedLock(
    IConnectionMultiplexer connectionMultiplexer,
    string redisKey,
    string resource,
    string token,
    ILogger logger) : IDistributedLock
{
    private const string ReleaseScript = """
        if redis.call("get", KEYS[1]) == ARGV[1] then
            return redis.call("del", KEYS[1])
        else
            return 0
        end
        """;

    private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer;
    private readonly string _redisKey = redisKey;
    private readonly string _token = token;
    private int _released;

    public string Resource { get; } = resource;

    public async Task ReleaseAsync(CancellationToken ct = default)
    {
        if (Interlocked.Exchange(ref _released, 1) == 1)
            return;

        var db = _connectionMultiplexer.GetDatabase();
        await db.ScriptEvaluateAsync(ReleaseScript, [_redisKey], [_token]);

        logger.LogInformation("Lock released for resource {Resource}", Resource);
    }

    public async ValueTask DisposeAsync() => await ReleaseAsync();
}
