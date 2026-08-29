namespace NovaCore.BuildingBlock.Application.Abstractions.Caching;

/// <summary>Whether a change means the cached value must be dropped, or merely that a newer
/// version now exists (currently unused by any publisher - see <see cref="ICacheChangeListener"/>).</summary>
public enum CacheChangeOperation
{
    Invalidate,
    Refresh
}

/// <summary>
/// Wire payload for cache synchronization Pub/Sub. Deliberately minimal - key/version/operation/
/// timestamp only, never the cached value itself, so Pub/Sub stays a lightweight "something
/// changed" signal rather than a data transport. Distinct from <c>IInternalEvent</c> (in-process
/// MediatR) and <c>IIntegrationEvent</c> (Kafka/outbox, durable business events) - this is neither;
/// it is a best-effort, ephemeral cache-sync signal, and TTL is the safety net for a message that
/// never arrives.
/// </summary>
public sealed record CacheChangeMessage(string Key, long Version, CacheChangeOperation Operation, DateTimeOffset OccurredAtUtc);
