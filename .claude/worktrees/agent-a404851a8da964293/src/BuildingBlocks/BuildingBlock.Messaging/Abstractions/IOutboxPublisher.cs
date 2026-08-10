namespace NovaCore.BuildingBlock.Messaging.Abstractions;

/// <summary>
/// Publishes outbox messages to the message broker with outbox-specific headers
/// (message-id for Inbox deduplication). Used by the OutboxRelayHostedService
/// to publish queued integration events.
/// </summary>
public interface IOutboxPublisher
{
    Task PublishOutboxMessageAsync(
        Guid messageId,
        string topic,
        string payload,
        string eventType,
        string correlationId,
        string? actorId,
        string actorType,
        CancellationToken ct = default);
}
