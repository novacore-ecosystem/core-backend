using NovaCore.BuildingBlock.Application.Abstractions.Caching;

namespace NovaCore.BuildingBlock.Infrastructure.Caching;

/// <summary>No-op publisher - the default for a service using layered caching without cross-instance
/// Pub/Sub synchronization. <c>AddRedisCacheChangePubSub</c> supersedes this registration.</summary>
internal sealed class NullCacheChangePublisher : ICacheChangePublisher
{
    public Task PublishAsync(string channel, CacheChangeMessage message, CancellationToken ct = default) => Task.CompletedTask;
}
