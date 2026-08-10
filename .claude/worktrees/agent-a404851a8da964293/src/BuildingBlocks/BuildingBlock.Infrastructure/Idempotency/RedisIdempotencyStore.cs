using NovaCore.BuildingBlock.Application.Abstractions.Idempotency;
using NovaCore.BuildingBlock.Application.Abstractions.Services;

namespace NovaCore.BuildingBlock.Infrastructure.Idempotency;

/// <summary>Redis-backed <see cref="IIdempotencyStore"/>, built on the existing <see cref="ICacheService"/> Redis path.</summary>
internal sealed class RedisIdempotencyStore(ICacheService cacheService) : IIdempotencyStore
{
    private const string KeyPrefix = "idempotency:";

    public Task<IdempotencyRecord?> GetAsync(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return cacheService.GetAsync<IdempotencyRecord>($"{KeyPrefix}{key}", ct);
    }

    public Task SaveAsync(string key, IdempotencyRecord record, TimeSpan expiration, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(record);
        return cacheService.SetAsync($"{KeyPrefix}{key}", record, expiration, ct);
    }
}
