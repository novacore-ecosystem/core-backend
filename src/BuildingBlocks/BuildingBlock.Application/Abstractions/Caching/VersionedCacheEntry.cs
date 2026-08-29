namespace NovaCore.BuildingBlock.Application.Abstractions.Caching;

/// <summary>
/// A cached value paired with its version. Keeping both together means a Redis/L1 round trip only
/// costs one fetch, and a Pub/Sub subscriber can compare its cached Version against an incoming
/// <see cref="CacheChangeMessage.Version"/> without a second lookup.
/// </summary>
public sealed record VersionedCacheEntry<TValue>(TValue Value, long Version, DateTimeOffset CachedAtUtc);
