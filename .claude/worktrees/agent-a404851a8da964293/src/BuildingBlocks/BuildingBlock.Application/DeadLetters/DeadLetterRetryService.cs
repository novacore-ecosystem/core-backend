using System.Diagnostics;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using NovaCore.BuildingBlock.Application.Abstractions.Idempotency;
using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.DeadLetters.Enums;
using NovaCore.BuildingBlock.Messaging.Abstractions;

namespace NovaCore.BuildingBlock.Application.DeadLetters;

public sealed class DeadLetterRetryService(
    IInboxStore inboxStore,
    IOutboxPublisher outboxPublisher,
    IDistributedLockProvider? lockProvider,
    ICurrentUserService currentUser,
    ILogger<DeadLetterRetryService> logger) : IDeadLetterRetryService
{
    private static readonly TimeSpan LockExpiration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(2);

    public async Task<DeadLetterRetryAttemptResult> RetryAsync(
        Guid inboxMessageId,
        CancellationToken ct = default)
    {
        var operatorId = currentUser.GetUserId()?.ToString();
        logger.LogInformation(
            "Dead-letter retry requested for InboxMessage {InboxMessageId} by {Operator}",
            inboxMessageId,
            operatorId ?? "unknown");

        // Acquire distributed lock
        var (@lock, conflict) = await AcquireLockAsync(inboxMessageId, ct);
        if (conflict)
            return new DeadLetterRetryAttemptResult(
                inboxMessageId,
                DeadLetterRetryOutcome.Conflict,
                null);

        await using var _ = @lock;

        // Requeue the dead-lettered message
        var requeue = await RequeueAsync(inboxMessageId, operatorId, ct);
        if (requeue.Outcome == InboxRequeueOutcome.NotFound)
            return new DeadLetterRetryAttemptResult(
                inboxMessageId,
                DeadLetterRetryOutcome.NotFound,
                null);
        if (requeue.Outcome == InboxRequeueOutcome.NotDeadLetter)
            return new DeadLetterRetryAttemptResult(
                inboxMessageId,
                DeadLetterRetryOutcome.NotDeadLetter,
                null);

        // Republish to the outbox
        return await PublishRetryAsync(inboxMessageId, requeue.Snapshot!, ct);
    }

    // ============================================================================
    // Locking
    // Defense-in-depth against thundering-herd bulk/retry-all calls hitting the same row -
    // RequeueDeadLetterAsync's conditional update already guarantees correctness on its own,
    // so the lock is skipped (not required) on services with no Redis/IDistributedLockProvider
    // registered rather than forcing a new infrastructure dependency onto them.
    // ============================================================================

    #region Locking

    private async Task<(IDistributedLock? Lock, bool Conflict)> AcquireLockAsync(
        Guid inboxMessageId,
        CancellationToken ct)
    {
        if (lockProvider is null)
            return (null, false);

        var lockKey = $"deadletter-retry:{inboxMessageId}";
        var @lock = await lockProvider.AcquireAsync(
            lockKey,
            LockExpiration,
            LockTimeout, ct);
        if (@lock is null)
        {
            logger.LogWarning(
                "Dead-letter retry conflict for InboxMessage {InboxMessageId}: already retrying",
                inboxMessageId);

            return (null, true);
        }

        return (@lock, false);
    }

    #endregion

    // ============================================================================
    // Requeue
    // Atomically flips the InboxMessage row back to Retrying, logging the attempt
    // once we know it's actually going ahead (row found and was DeadLetter).
    // ============================================================================

    #region Requeue

    private async Task<InboxRequeueResult> RequeueAsync(
        Guid inboxMessageId,
        string? operatorId,
        CancellationToken ct)
    {
        var requeue = await inboxStore.RequeueDeadLetterAsync(
            inboxMessageId,
            operatorId,
            ct);
        if (requeue.Outcome != InboxRequeueOutcome.Requeued)
            return requeue;

        var snapshot = requeue.Snapshot!;
        logger.LogInformation(
            "Dead-letter retry started for InboxMessage {InboxMessageId} ({ConsumerName}/{Topic}), attempt #{RetryNumber}",
            inboxMessageId,
            snapshot.ConsumerName,
            snapshot.Topic,
            requeue.RetryNumber);

        return requeue;
    }

    #endregion

    // ============================================================================
    // Publish
    // Republishes the dead-lettered message to the outbox. A publish failure reverts
    // the row back to DeadLetter so the operator can retry again later.
    // ============================================================================

    #region Publish

    private async Task<DeadLetterRetryAttemptResult> PublishRetryAsync(
        Guid inboxMessageId,
        InboxMessageSnapshot snapshot,
        CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var (eventType, correlationId, actorId, actorType) = ParseHeaders(snapshot.HeadersJson);
            await outboxPublisher.PublishOutboxMessageAsync(
                snapshot.MessageId,
                snapshot.Topic,
                snapshot.Payload,
                eventType,
                correlationId,
                actorId,
                actorType,
                ct);

            logger.LogInformation(
                "Dead-letter retry republished InboxMessage {InboxMessageId} to {Topic} in {DurationMs}ms - " +
                "outcome (Succeeded/FailedAgain) will be recorded when the redelivered message is next processed",
                inboxMessageId,
                snapshot.Topic,
                stopwatch.ElapsedMilliseconds);

            return new DeadLetterRetryAttemptResult(
                inboxMessageId,
                DeadLetterRetryOutcome.Succeeded,
                null);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Dead-letter retry failed to republish InboxMessage {InboxMessageId} after {DurationMs}ms - reverting to DeadLetter",
                inboxMessageId,
                stopwatch.ElapsedMilliseconds);

            await inboxStore.RevertFailedRequeueAsync(
                inboxMessageId,
                ex.Message,
                ct);
            return new DeadLetterRetryAttemptResult(
                inboxMessageId,
                DeadLetterRetryOutcome.PublishFailed,
                ex.Message);
        }
    }

    /// <summary>
    /// Recovers the fields KafkaOutboxPublisher originally wrote as headers (see
    /// NovaCore.BuildingBlock.Messaging.Kafka.Publishers.KafkaOutboxPublisher) from the dedup-header JSON
    /// captured on first delivery, so the republish carries the same event-type/correlation/actor
    /// metadata as the original publish.
    /// </summary>
    private static (string EventType, string CorrelationId, string? ActorId, string ActorType) ParseHeaders(string headersJson)
    {
        var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersJson) ?? [];

        headers.TryGetValue("event-type", out var eventType);
        headers.TryGetValue("correlation-id", out var correlationId);
        headers.TryGetValue("actor-type", out var actorType);
        headers.TryGetValue("actor-id", out var actorId);

        return (
            eventType ?? string.Empty,
            correlationId ?? Guid.NewGuid().ToString(),
            actorId,
            actorType ?? "System");
    }

    #endregion
}
