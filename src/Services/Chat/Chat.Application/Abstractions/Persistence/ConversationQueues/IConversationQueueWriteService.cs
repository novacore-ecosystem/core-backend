namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationQueues;

public interface IConversationQueueWriteService
{
    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task CreateAsync(ConversationQueue queue, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
