using NovaCore.BuildingBlock.Application.Abstractions.Outbox;

namespace NovaCore.Product.Persistence.Reliability.Inbox;

/// <summary>
/// Application-level adapter: delegates to the generic EF store, translating between the two
/// layers' independently-defined Inbox DTOs/enums (same convention as Outbox's dual snapshot).
/// </summary>
public sealed class InboxStore(NovaCore.BuildingBlock.Persistence.Inbox.IInboxStore primitiveStore) : NovaCore.BuildingBlock.Application.Abstractions.Outbox.IInboxStore
{
    private readonly NovaCore.BuildingBlock.Persistence.Inbox.IInboxStore _primitiveStore = primitiveStore;

    public async Task<InboxAttemptDecision> BeginAttemptAsync(
        Guid messageId,
        string consumerName,
        string topic,
        string payload,
        string headersJson,
        CancellationToken ct = default)
    {
        var decision = await _primitiveStore.BeginAttemptAsync(
            messageId,
            consumerName,
            topic,
            payload,
            headersJson,
            ct);
        return ToApplication(decision);
    }

    public Task CompleteAttemptAsync(Guid messageId, string consumerName, CancellationToken ct = default) =>
        _primitiveStore.CompleteAttemptAsync(messageId, consumerName, ct);

    public async Task<InboxFailureOutcome> FailAttemptAsync(
        Guid messageId,
        string consumerName,
        string error,
        InboxRetryPolicy policy,
        CancellationToken ct = default)
    {
        var primitivePolicy = new NovaCore.BuildingBlock.Persistence.Inbox.InboxRetryPolicy(
            policy.MaxRetryCount,
            policy.InitialRetryDelay,
            policy.RetryBackoffMultiplier,
            policy.MaximumRetryDelay);

        var outcome = await _primitiveStore.FailAttemptAsync(
            messageId,
            consumerName,
            error,
            primitivePolicy,
            ct);
        return ToApplication(outcome);
    }

    public async Task<IReadOnlyList<InboxMessageSnapshot>> GetDueForRetryAsync(int batchSize, CancellationToken ct = default)
    {
        var rows = await _primitiveStore.GetDueForRetryAsync(batchSize, ct);
        return [.. rows.Select(ToApplication)];
    }

    public Task<int> DeleteProcessedBeforeAsync(DateTime olderThanUtc, int batchSize, CancellationToken ct = default) =>
        _primitiveStore.DeleteProcessedBeforeAsync(olderThanUtc, batchSize, ct);

    public async Task<IReadOnlyList<InboxDeadLetterSummary>> GetDeadLetterSummaryAsync(CancellationToken ct = default)
    {
        var rows = await _primitiveStore.GetDeadLetterSummaryAsync(ct);
        return [.. rows.Select(ToApplication)];
    }

    public async Task<InboxRequeueResult> RequeueDeadLetterAsync(Guid inboxMessageId, string? operatorId, CancellationToken ct = default)
    {
        var result = await _primitiveStore.RequeueDeadLetterAsync(inboxMessageId, operatorId, ct);
        return ToApplication(result);
    }

    public async Task<IReadOnlyList<InboxRetryHistoryEntry>> GetRetryHistoryAsync(Guid inboxMessageId, CancellationToken ct = default)
    {
        var rows = await _primitiveStore.GetRetryHistoryAsync(inboxMessageId, ct);
        return [.. rows.Select(ToApplication)];
    }

    public Task RevertFailedRequeueAsync(Guid inboxMessageId, string error, CancellationToken ct = default) =>
        _primitiveStore.RevertFailedRequeueAsync(inboxMessageId, error, ct);

    private static InboxAttemptDecision ToApplication(NovaCore.BuildingBlock.Persistence.Inbox.InboxAttemptDecision decision) => decision switch
    {
        NovaCore.BuildingBlock.Persistence.Inbox.InboxAttemptDecision.Proceed => InboxAttemptDecision.Proceed,
        NovaCore.BuildingBlock.Persistence.Inbox.InboxAttemptDecision.AlreadyProcessed => InboxAttemptDecision.AlreadyProcessed,
        NovaCore.BuildingBlock.Persistence.Inbox.InboxAttemptDecision.DeadLettered => InboxAttemptDecision.DeadLettered,
        NovaCore.BuildingBlock.Persistence.Inbox.InboxAttemptDecision.NotDueYet => InboxAttemptDecision.NotDueYet,
        _ => throw new ArgumentOutOfRangeException(nameof(decision), decision, null),
    };

    private static InboxFailureOutcome ToApplication(NovaCore.BuildingBlock.Persistence.Inbox.InboxFailureOutcome outcome) => outcome switch
    {
        NovaCore.BuildingBlock.Persistence.Inbox.InboxFailureOutcome.AlreadyCommitted => InboxFailureOutcome.AlreadyCommitted,
        NovaCore.BuildingBlock.Persistence.Inbox.InboxFailureOutcome.WillRetry => InboxFailureOutcome.WillRetry,
        NovaCore.BuildingBlock.Persistence.Inbox.InboxFailureOutcome.DeadLettered => InboxFailureOutcome.DeadLettered,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null),
    };

    private static InboxMessageStatus ToApplication(NovaCore.BuildingBlock.Persistence.Inbox.InboxMessageStatus status) => status switch
    {
        NovaCore.BuildingBlock.Persistence.Inbox.InboxMessageStatus.Pending => InboxMessageStatus.Pending,
        NovaCore.BuildingBlock.Persistence.Inbox.InboxMessageStatus.Retrying => InboxMessageStatus.Retrying,
        NovaCore.BuildingBlock.Persistence.Inbox.InboxMessageStatus.Processed => InboxMessageStatus.Processed,
        NovaCore.BuildingBlock.Persistence.Inbox.InboxMessageStatus.DeadLetter => InboxMessageStatus.DeadLetter,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };

    private static InboxMessageSnapshot ToApplication(NovaCore.BuildingBlock.Persistence.Inbox.InboxMessageSnapshot snapshot) => new(
        snapshot.MessageId,
        snapshot.ConsumerName,
        snapshot.Topic,
        snapshot.Payload,
        snapshot.HeadersJson,
        ToApplication(snapshot.Status),
        snapshot.RetryCount,
        snapshot.CreatedAt,
        snapshot.ProcessedAt,
        snapshot.NextRetryAt,
        snapshot.LastRetryAt,
        snapshot.LastError);

    private static InboxDeadLetterSummary ToApplication(NovaCore.BuildingBlock.Persistence.Inbox.InboxDeadLetterSummary summary) => new(
        summary.ConsumerName, summary.Topic, summary.Count, summary.OldestDeadLetteredAt);

    private static InboxRequeueResult ToApplication(NovaCore.BuildingBlock.Persistence.Inbox.InboxRequeueResult result) => new(
        result.Outcome switch
        {
            NovaCore.BuildingBlock.Persistence.Inbox.InboxRequeueOutcome.Requeued => InboxRequeueOutcome.Requeued,
            NovaCore.BuildingBlock.Persistence.Inbox.InboxRequeueOutcome.NotFound => InboxRequeueOutcome.NotFound,
            NovaCore.BuildingBlock.Persistence.Inbox.InboxRequeueOutcome.NotDeadLetter => InboxRequeueOutcome.NotDeadLetter,
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Outcome, null),
        },
        result.Snapshot is null ? null : ToApplication(result.Snapshot),
        result.RetryNumber);

    private static InboxRetryHistoryEntry ToApplication(NovaCore.BuildingBlock.Persistence.Inbox.InboxRetryHistoryEntry entry) => new(
        entry.Id, entry.InboxMessageId, entry.MessageId, entry.ConsumerName, entry.Topic, entry.RetryNumber,
        entry.StartedAt, entry.FinishedAt, entry.DurationMs, entry.Operator,
        entry.Result switch
        {
            NovaCore.BuildingBlock.Persistence.Inbox.InboxRetryHistoryResult.Retrying => InboxRetryHistoryResult.Retrying,
            NovaCore.BuildingBlock.Persistence.Inbox.InboxRetryHistoryResult.Succeeded => InboxRetryHistoryResult.Succeeded,
            NovaCore.BuildingBlock.Persistence.Inbox.InboxRetryHistoryResult.FailedAgain => InboxRetryHistoryResult.FailedAgain,
            NovaCore.BuildingBlock.Persistence.Inbox.InboxRetryHistoryResult.Cancelled => InboxRetryHistoryResult.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(entry), entry.Result, null),
        },
        entry.Exception);
}
