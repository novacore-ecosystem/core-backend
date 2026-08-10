namespace NovaCore.BuildingBlock.Persistence.Inbox;

/// <summary>
/// Read-only projection of an inbox row. Returned by <see cref="IInboxStore"/> instead of
/// an entity type, since the store must not expose provider-specific (EF, Mongo, ...) models.
/// Payload/Topic/Headers are captured on first sight so a background retry can re-invoke the
/// consumer without needing the original Kafka message to still be available.
/// </summary>
public sealed record InboxMessageSnapshot(
    Guid MessageId,
    string ConsumerName,
    string Topic,
    string Payload,
    string HeadersJson,
    InboxMessageStatus Status,
    int RetryCount,
    DateTime CreatedAt,
    DateTime? ProcessedAt,
    DateTime? NextRetryAt,
    DateTime? LastRetryAt,
    string? LastError);
