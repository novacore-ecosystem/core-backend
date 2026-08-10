namespace NovaCore.BuildingBlock.Persistence.Inbox;

/// <summary>
/// Result of <see cref="IInboxStore.RequeueDeadLetterAsync"/>. <paramref name="Snapshot"/> is
/// only populated when <paramref name="Outcome"/> is <see cref="InboxRequeueOutcome.Requeued"/> -
/// it carries everything the caller needs to republish the message through the normal Kafka
/// pipeline (see NovaCore.BuildingBlock.Messaging.Abstractions.IOutboxPublisher).
/// </summary>
public sealed record InboxRequeueResult(InboxRequeueOutcome Outcome, InboxMessageSnapshot? Snapshot, int RetryNumber);
