namespace NovaCore.BuildingBlock.Persistence.Inbox;

/// <summary>
/// Outcome of <see cref="IInboxStore.BeginAttemptAsync"/> - tells the caller whether it should
/// actually invoke the consumer handler for this (messageId, consumerName) pair.
/// </summary>
public enum InboxAttemptDecision
{
    /// <summary>No prior row, or a prior Retrying row whose NextRetryAt has arrived - proceed to invoke the handler.</summary>
    Proceed,

    /// <summary>Already Processed - the handler must not run again.</summary>
    AlreadyProcessed,

    /// <summary>Permanently failed after exceeding MaxRetryCount - never retried automatically.</summary>
    DeadLettered,

    /// <summary>Retrying, but NextRetryAt is still in the future.</summary>
    NotDueYet,
}
