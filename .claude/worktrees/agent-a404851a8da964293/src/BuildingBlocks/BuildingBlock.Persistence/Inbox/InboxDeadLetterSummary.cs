namespace NovaCore.BuildingBlock.Persistence.Inbox;

/// <summary>
/// Aggregate view of DeadLetter rows sharing the same (ConsumerName, Topic), returned by
/// <see cref="IInboxStore.GetDeadLetterSummaryAsync"/> so a monitoring job can alert on stuck
/// messages without loading full row payloads.
/// </summary>
public sealed record InboxDeadLetterSummary(
    string ConsumerName,
    string Topic,
    int Count,
    DateTime OldestDeadLetteredAt);
