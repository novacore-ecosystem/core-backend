using System.Text.Json;
using System.Threading.Channels;

using NovaCore.BuildingBlock.Application.Abstractions.Caching;
using NovaCore.BuildingBlock.SharedKernel.Serialization;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace NovaCore.BuildingBlock.Infrastructure.PubSub;

/// <summary>
/// Subscribes to every distinct channel named by a registered <see cref="ICacheChangeListener"/> and
/// fans incoming messages out to the matching listeners. The Redis message callback only deserializes
/// and enqueues (never awaits/blocks) - a single dedicated loop drains the queue and dispatches, so
/// slow or faulty listener code never stalls Redis's own dispatch thread. Relies on
/// StackExchange.Redis re-issuing subscriptions automatically after a transient reconnect; the
/// ConnectionFailed/ConnectionRestored hooks below are for logging only, not control flow.
/// </summary>
internal sealed class RedisCacheChangeSubscriberHostedService(
    IConnectionMultiplexer connectionMultiplexer,
    IEnumerable<ICacheChangeListener> listeners,
    ILogger<RedisCacheChangeSubscriberHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var listenersByChannel = GroupListenersByChannel(listeners);

        if (listenersByChannel.Count == 0)
        {
            logger.LogInformation("Cache-change subscriber starting with no registered listeners - nothing to subscribe to");
            return;
        }

        logger.LogInformation("Cache-change subscriber starting for channels: {Channels}", string.Join(", ", listenersByChannel.Keys));

        var incoming = Channel.CreateBounded<(string ChannelName, RedisValue Payload)>(new BoundedChannelOptions(1024)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

        var subscriber = connectionMultiplexer.GetSubscriber();
        var queues = new List<ChannelMessageQueue>(listenersByChannel.Count);

        foreach (var channelName in listenersByChannel.Keys)
        {
            var queue = await subscriber.SubscribeAsync(RedisChannel.Literal(channelName));
            queue.OnMessage(msg => incoming.Writer.TryWrite((channelName, msg.Message)));
            queues.Add(queue);
        }

        connectionMultiplexer.ConnectionFailed += OnConnectionFailed;
        connectionMultiplexer.ConnectionRestored += OnConnectionRestored;

        try
        {
            await foreach (var (channelName, payload) in incoming.Reader.ReadAllAsync(stoppingToken))
                Dispatch(channelName, payload, listenersByChannel);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Cache-change subscriber stopping");
        }
        finally
        {
            connectionMultiplexer.ConnectionFailed -= OnConnectionFailed;
            connectionMultiplexer.ConnectionRestored -= OnConnectionRestored;

            foreach (var queue in queues)
            {
                try
                {
                    await queue.UnsubscribeAsync();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to unsubscribe a cache-change channel during shutdown");
                }
            }
        }
    }

    /// <summary>Groups listeners by <see cref="ICacheChangeListener.Channel"/> - extracted as its own
    /// pure, static, directly-testable step (no Redis involved) from the subscribe/dispatch loop.</summary>
    internal static IReadOnlyDictionary<string, IReadOnlyList<ICacheChangeListener>> GroupListenersByChannel(IEnumerable<ICacheChangeListener> listeners)
        => listeners
            .GroupBy(l => l.Channel, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, IReadOnlyList<ICacheChangeListener> (g) => [.. g], StringComparer.Ordinal);

    /// <summary>Deserializes and fans one raw message out to the listeners registered for its
    /// channel, isolating one listener's failure from the others. Internal (not private) so it can
    /// be exercised directly in tests without needing a real Redis subscription - StackExchange.Redis's
    /// <see cref="ChannelMessageQueue"/> is a sealed, only-internally-constructible type, so the
    /// subscribe-to-dispatch wiring itself is not unit-testable in isolation.</summary>
    internal void Dispatch(string channelName, RedisValue payload, IReadOnlyDictionary<string, IReadOnlyList<ICacheChangeListener>> listenersByChannel)
    {
        if (!listenersByChannel.TryGetValue(channelName, out var channelListeners))
            return;

        CacheChangeMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<CacheChangeMessage>(payload.ToString(), JsonSerializerConfiguration.Default);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to deserialize a cache-change message on channel {Channel}", channelName);
            return;
        }

        if (message is null)
            return;

        foreach (var listener in channelListeners)
        {
            try
            {
                listener.OnCacheChange(message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Cache-change listener threw while handling a message on channel {Channel}", channelName);
            }
        }
    }

    private void OnConnectionFailed(object? sender, ConnectionFailedEventArgs e)
        => logger.LogWarning(e.Exception, "Redis connection failed ({FailureType}) - cache-change subscriptions resume automatically once reconnected", e.FailureType);

    private void OnConnectionRestored(object? sender, ConnectionFailedEventArgs e)
        => logger.LogInformation("Redis connection restored - cache-change subscriptions active");
}
