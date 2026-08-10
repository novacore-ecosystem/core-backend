namespace NovaCore.BuildingBlock.Persistence.Inbox;

/// <summary>Result of one manually-triggered retry attempt, as recorded in the append-only history.</summary>
public enum InboxRetryHistoryResult
{
    /// <summary>Requeued and republished; the normal consumer pipeline hasn't reported an outcome yet.</summary>
    Retrying,

    /// <summary>The redelivered message was processed successfully.</summary>
    Succeeded,

    /// <summary>The redelivered message failed again (row may or may not be back in DeadLetter).</summary>
    FailedAgain,

    /// <summary>Reserved for a future cancel-in-flight-retry action; never set today.</summary>
    Cancelled,
}

/// <summary>
/// One row per manually-triggered retry attempt of a dead-lettered message. Never overwritten -
/// only inserted (on requeue) and later closed out in place by setting FinishedAt/Result once,
/// so the history is a genuine append-only audit trail across repeated retries of the same row.
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
