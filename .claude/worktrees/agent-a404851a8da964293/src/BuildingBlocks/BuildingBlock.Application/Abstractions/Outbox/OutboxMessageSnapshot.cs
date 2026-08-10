namespace NovaCore.BuildingBlock.Application.Abstractions.Outbox;

/// <summary>
/// Read-only projection of an outbox row, translated from the primitive
/// NovaCore.BuildingBlock.Persistence.Outbox.OutboxMessageSnapshot by the per-service adapter.
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
