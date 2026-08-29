namespace NovaCore.BuildingBlock.Application.Abstractions.Caching;

/// <summary>Receives cache-sync messages for one channel. Implemented by the layered cache itself
/// (registered as both <see cref="IVersionedCache{TValue}"/> and this interface, one instance per
/// <c>TValue</c>) so a Pub/Sub subscriber can fan an incoming message out to every cache listening
/// on that channel without knowing anything about <c>TValue</c>.</summary>
public interface ICacheChangeListener
{
    string Channel { get; }

    /// <summary>Called synchronously by the subscriber's dispatch loop - implementations must stay
    /// non-blocking (local dictionary eviction only, no I/O) since this runs off the message queue,
    /// not a request thread.</summary>
    void OnCacheChange(CacheChangeMessage message);
}
