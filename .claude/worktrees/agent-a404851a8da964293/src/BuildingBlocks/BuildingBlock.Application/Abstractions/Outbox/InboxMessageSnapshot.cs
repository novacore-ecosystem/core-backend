namespace NovaCore.BuildingBlock.Application.Abstractions.Outbox;

/// <summary>
/// Read-only projection of an inbox row, translated from the primitive
/// NovaCore.BuildingBlock.Persistence.Inbox.InboxMessageSnapshot by the per-service adapter.
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
