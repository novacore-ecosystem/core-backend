namespace NovaCore.BuildingBlock.Application.Abstractions.Outbox;

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

/// <summary>
/// Outcome of <see cref="IInboxStore.FailAttemptAsync"/>.
/// </summary>
public enum InboxFailureOutcome
{
    /// <summary>
    /// The consumer's own persistence already committed - and the optimistic Processed marker
    /// staged by BeginAttemptAsync committed in that same transaction - before the exception was
    /// observed here. The row is left Processed: flipping it back to Retrying would risk running
    /// already-committed business logic again on the next attempt.
    /// </summary>
    AlreadyCommitted,

    /// <summary>Nothing committed. RetryCount incremented, row left Retrying with a new NextRetryAt.</summary>
    WillRetry,

    /// <summary>Nothing committed. RetryCount exceeded MaxRetryCount, row moved to DeadLetter.</summary>
    DeadLettered,
}

/// <summary>
/// Dedup + retry tracking for incoming integration events, keyed by (messageId, consumerName) so
/// multiple consumers on the same topic don't block each other. Used generically by
/// IntegrationEventConsumerRegistry / InboxRetryHostedService - individual consumers don't need
/// to know this exists.
/// </summary>
public interface IInboxStore
{
    /// <summary>
    /// Looks up the (messageId, consumerName) row and decides whether the caller should invoke
    /// the consumer handler. When the decision is Proceed, the row is created (if absent) or
    /// reused, Topic/Payload/Headers are (re)captured for a possible future retry, and the row is
    /// optimistically staged as Processed on the current unit of work's change tracker - NOT
    /// saved. If the caller's own persistence (invoked between this call and
    /// CompleteAttemptAsync/FailAttemptAsync) issues its own SaveChanges, that same call commits
    /// the Processed marker atomically with the business change. CompleteAttemptAsync/
    /// FailAttemptAsync must be called afterward to finalize (or correct) the outcome.
    /// </summary>
    Task<InboxAttemptDecision> BeginAttemptAsync(
        Guid messageId,
        string consumerName,
        string topic,
        string payload,
        string headersJson,
        CancellationToken ct = default);

    /// <summary>
    /// Call after the handler invoked following a Proceed decision completes successfully.
    /// Flushes the optimistic Processed marker if it wasn't already committed by the handler's
    /// own SaveChanges.
    /// </summary>
    Task CompleteAttemptAsync(Guid messageId, string consumerName, CancellationToken ct = default);

    /// <summary>
    /// Call after the handler invoked following a Proceed decision throws. See
    /// <see cref="InboxFailureOutcome"/> for the three possible outcomes.
    /// </summary>
    Task<InboxFailureOutcome> FailAttemptAsync(
        Guid messageId,
        string consumerName,
        string error,
        InboxRetryPolicy policy,
        CancellationToken ct = default);

    /// <summary>Rows currently Retrying whose NextRetryAt has arrived, oldest first.</summary>
    Task<IReadOnlyList<InboxMessageSnapshot>> GetDueForRetryAsync(int batchSize, CancellationToken ct = default);

    /// <summary>
    /// Delete one batch of Processed rows older than <paramref name="olderThanUtc"/>. Never
    /// touches Pending/Retrying/DeadLetter rows. Returns the number deleted, so the caller
    /// (InboxCleanupJob) can loop until a batch comes back short of batchSize.
    /// </summary>
    Task<int> DeleteProcessedBeforeAsync(DateTime olderThanUtc, int batchSize, CancellationToken ct = default);

    /// <summary>
    /// Aggregate counts of DeadLetter rows grouped by (ConsumerName, Topic), for periodic ops
    /// monitoring. Never returns row payloads - just enough to alert on and locate the rows.
    /// </summary>
    Task<IReadOnlyList<InboxDeadLetterSummary>> GetDeadLetterSummaryAsync(CancellationToken ct = default);

    /// <summary>
    /// Atomically flips a row from DeadLetter back to Retrying and records a new retry-history
    /// entry. See NovaCore.BuildingBlock.Persistence.Inbox.IInboxStore.RequeueDeadLetterAsync for the
    /// concurrency-safety rationale.
    /// </summary>
    Task<InboxRequeueResult> RequeueDeadLetterAsync(Guid inboxMessageId, string? operatorId, CancellationToken ct = default);

    /// <summary>Retry-history entries for one Inbox row, most recent first.</summary>
    Task<IReadOnlyList<InboxRetryHistoryEntry>> GetRetryHistoryAsync(Guid inboxMessageId, CancellationToken ct = default);

    /// <summary>
    /// Compensating action for a successful RequeueDeadLetterAsync whose subsequent Kafka publish
    /// then failed. Reverts the row to DeadLetter and closes the open retry-history entry as
    /// FailedAgain.
    /// </summary>
    Task RevertFailedRequeueAsync(Guid inboxMessageId, string error, CancellationToken ct = default);
}
