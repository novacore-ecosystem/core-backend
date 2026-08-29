using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Caching;
using NovaCore.BuildingBlock.SharedKernel.Serialization;

using StackExchange.Redis;

namespace NovaCore.BuildingBlock.Infrastructure.PubSub;

/// <summary>Publishes cache-change signals over Redis Pub/Sub. Reuses the shared
/// <see cref="IConnectionMultiplexer"/> singleton (no second Redis connection) and the same JSON
/// serialization convention as <c>RedisCacheService</c>.</summary>
internal sealed class RedisCacheChangePublisher(IConnectionMultiplexer connectionMultiplexer) : ICacheChangePublisher
{
    public async Task PublishAsync(string channel, CacheChangeMessage message, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channel);
        ArgumentNullException.ThrowIfNull(message);

        var payload = JsonSerializer.Serialize(message, JsonSerializerConfiguration.Default);
        var subscriber = connectionMultiplexer.GetSubscriber();
        await subscriber.PublishAsync(RedisChannel.Literal(channel), payload);
    }
}
