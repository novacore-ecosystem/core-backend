namespace NovaCore.BuildingBlock.Persistence.Outbox;

/// <summary>
/// Read-only projection of an outbox row. Returned by <see cref="IOutboxStore"/> instead of
/// an entity type, since the store must not expose provider-specific (EF, Mongo, ...) models.
/// </summary>
public sealed record OutboxMessageSnapshot(
    Guid Id,
    string EventType,
    string Topic,
    string Payload,
    string CorrelationId,
    string? ActorId,
    string ActorType,
    DateTime CreatedAt,
    DateTime? ProcessedAt,
    string? Error,
    int RetryCount);
