namespace NovaCore.BuildingBlock.Application.Abstractions.Outbox;

/// <summary>
/// Outcome of <see cref="IInboxStore.RequeueDeadLetterAsync"/>.
/// </summary>
public enum InboxRequeueOutcome
{
    /// <summary>Row existed, was DeadLetter, and was atomically flipped back to Retrying.</summary>
    Requeued,

    /// <summary>No row exists for the given id.</summary>
    NotFound,

    /// <summary>Row exists but is not currently DeadLetter (already requeued, or never dead-lettered).</summary>
    NotDeadLetter,
}

/// <summary>
/// Result of <see cref="IInboxStore.RequeueDeadLetterAsync"/>. <paramref name="Snapshot"/> is
/// only populated when <paramref name="Outcome"/> is <see cref="InboxRequeueOutcome.Requeued"/>.
/// </summary>
public sealed record InboxRequeueResult(InboxRequeueOutcome Outcome, InboxMessageSnapshot? Snapshot, int RetryNumber);
