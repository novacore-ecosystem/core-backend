using NovaCore.BuildingBlock.Persistence.Inbox;

namespace NovaCore.BuildingBlock.Persistence.Ef.Inbox;

/// <summary>
/// Append-only record of one manually-triggered retry attempt against a dead-lettered
/// InboxMessage row. Rows are never overwritten - RequeueDeadLetterAsync inserts a new one
/// (Result = Retrying), and the matching CompleteAttemptAsync/FailAttemptAsync call that later
/// observes this InboxMessageId closes it out in place (Succeeded/FailedAgain), exactly once.
/// </summary>
public sealed class InboxRetryHistory
{
    public Guid Id { get; private set; }
    public Guid InboxMessageId { get; private set; }
    public Guid MessageId { get; private set; }
    public string ConsumerName { get; private set; } = string.Empty;
    public string Topic { get; private set; } = string.Empty;
    public int RetryNumber { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public long? DurationMs { get; private set; }
    public string? Operator { get; private set; }
    public InboxRetryHistoryResult Result { get; private set; }
    public string? Exception { get; private set; }

    private InboxRetryHistory() { }

    public static InboxRetryHistory Start(
        Guid inboxMessageId,
        Guid messageId,
        string consumerName,
        string topic,
        int retryNumber,
        string? operatorId)
    {
        return new InboxRetryHistory
        {
            Id = Guid.CreateVersion7(),
            InboxMessageId = inboxMessageId,
            MessageId = messageId,
            ConsumerName = consumerName,
            Topic = topic,
            RetryNumber = retryNumber,
            StartedAt = DateTime.UtcNow,
            Operator = operatorId,
            Result = InboxRetryHistoryResult.Retrying,
        };
    }

    public void Close(InboxRetryHistoryResult result, string? exception)
    {
        FinishedAt = DateTime.UtcNow;
        DurationMs = (long)(FinishedAt.Value - StartedAt).TotalMilliseconds;
        Result = result;
        Exception = exception;
    }
}
