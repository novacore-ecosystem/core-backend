using NovaCore.BuildingBlock.Application.Abstractions.Outbox;

namespace NovaCore.BuildingBlock.Application.Abstractions.DeadLetters;

/// <summary>Row-level projection for the dead-letter list/search API - no Payload/HeadersJson (see detail response).</summary>
public sealed record DeadLetterListItemResponse(
    Guid Id,
    Guid MessageId,
    string ConsumerName,
    string Topic,
    InboxMessageStatus Status,
    int RetryCount,
    DateTime CreatedAt,
    DateTime? LastRetryAt,
    string? LastError);

/// <summary>Full detail for one dead-lettered row, including its append-only retry history.</summary>
public sealed record DeadLetterDetailResponse(
    Guid Id,
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
    string? LastError,
    IReadOnlyList<InboxRetryHistoryEntry> RetryHistory);
