using NovaCore.BuildingBlock.Messaging.Abstractions;

using Microsoft.Extensions.Logging;

namespace NovaCore.BuildingBlock.Messaging.Services;

/// <summary>
/// Filters registered integration event consumers by topic and dispatches messages to them.
/// Broker adapters (e.g. KafkaFlow message handlers) resolve this per-message from a DI scope.
/// Delegates the actual Inbox dedup/retry bookkeeping to <c>executeWithInboxAsync</c> (built by
/// NovaCore.BuildingBlock.Infrastructure) so this project never needs to depend on the Application or
/// Persistence layers - it just supplies the callback that invokes the consumer.
/// </summary>
public sealed class IntegrationEventConsumerRegistry(
    IEnumerable<IIntegrationEventConsumer> consumers,
    Func<InboxDispatchContext, Func<Task>, CancellationToken, Task> executeWithInboxAsync,
    ILogger<IntegrationEventConsumerRegistry> logger)
{
    public IReadOnlyCollection<string> Topics =>
        [.. consumers.SelectMany(c => c.Topics).Distinct()];

    public async Task DispatchAsync(
        string topic,
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        // Only messages relayed through the Outbox carry this header. If it's absent
        // (e.g. a direct-publish path not yet migrated to Outbox), dedup/retry tracking is
        // skipped rather than risking a false-positive skip.
        var messageId = Guid.Empty;
        var hasMessageId = headers.TryGetValue("message-id", out var messageIdValue)
            && Guid.TryParse(messageIdValue, out messageId);

        foreach (var consumer in consumers.Where(c => c.Topics.Contains(topic)))
        {
            var consumerName = consumer.GetType().Name;

            if (!hasMessageId)
            {
                try
                {
                    await consumer.HandleAsync(message, headers, ct);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in consumer {ConsumerType} for topic {Topic}", consumerName, topic);
                }

                continue;
            }

            var dispatchContext = new InboxDispatchContext(messageId, consumerName, topic, message, headers);
            await executeWithInboxAsync(dispatchContext, () => consumer.HandleAsync(message, headers, ct), ct);
        }
    }
}
