namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationQueues;

public interface IConversationQueueItemWriteService
{
    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task EnqueueAsync(Guid queueId, Guid conversationId, int priority, CancellationToken ct = default);

    /// <summary>Marks the conversation's current Waiting item Assigned. Commits via bare SaveChangesAsync.</summary>
    Task MarkAssignedAsync(Guid conversationId, CancellationToken ct = default);
}
