namespace NovaCore.BuildingBlock.Application.Abstractions.Caching;

/// <summary>Publishes a cache-sync signal to a named channel. The default implementation
/// (<c>NullCacheChangePublisher</c>) is a no-op for services that use layered caching without
/// cross-instance synchronization; <c>AddRedisCacheChangePubSub</c> supersedes it with a real
/// Redis Pub/Sub publisher.</summary>
public interface ICacheChangePublisher
{
    Task PublishAsync(string channel, CacheChangeMessage message, CancellationToken ct = default);
}
