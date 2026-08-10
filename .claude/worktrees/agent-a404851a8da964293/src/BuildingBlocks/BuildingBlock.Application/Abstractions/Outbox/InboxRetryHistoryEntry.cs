namespace NovaCore.BuildingBlock.Application.Abstractions.Outbox;

/// <summary>Result of one manually-triggered retry attempt, as recorded in the append-only history.</summary>
public enum InboxRetryHistoryResult
{
    Retrying,
    Succeeded,
    FailedAgain,
    Cancelled,
}

/// <summary>
/// One row per manually-triggered retry attempt of a dead-lettered message. Never overwritten -
/// only inserted (on requeue) and later closed out in place once.
/// </summary>
public sealed record InboxRetryHistoryEntry(
    Guid Id,
    Guid InboxMessageId,
    Guid MessageId,
    string ConsumerName,
    string Topic,
    int RetryNumber,
    DateTime StartedAt,
    DateTime? FinishedAt,
    long? DurationMs,
    string? Operator,
    InboxRetryHistoryResult Result,
    string? Exception);
