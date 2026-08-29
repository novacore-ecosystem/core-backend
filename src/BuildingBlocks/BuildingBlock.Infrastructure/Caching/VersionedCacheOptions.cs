namespace NovaCore.BuildingBlock.Infrastructure.Caching;

/// <summary>Per-<c>TValue</c> configuration for a <see cref="LayeredVersionedCache{TValue}"/>,
/// registered via <c>AddVersionedCache&lt;TValue&gt;</c>. TTLs are a safety net for a missed
/// Pub/Sub message, not the primary consistency mechanism - invalidation is expected to arrive
/// via <see cref="NovaCore.BuildingBlock.Application.Abstractions.Caching.ICacheChangePublisher"/>.</summary>
public sealed class VersionedCacheOptions<TValue>
{
    public required string CacheName { get; set; }

    public required string ChannelName { get; set; }

    public TimeSpan LocalMemoryTtl { get; set; } = TimeSpan.FromMinutes(2);

    public TimeSpan RedisTtl { get; set; } = TimeSpan.FromMinutes(30);

    public TimeSpan LockExpiration { get; set; } = TimeSpan.FromSeconds(10);

    public TimeSpan LockTimeout { get; set; } = TimeSpan.FromSeconds(3);
}
