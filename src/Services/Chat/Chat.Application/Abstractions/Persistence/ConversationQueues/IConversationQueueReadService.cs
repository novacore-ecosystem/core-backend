namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationQueues;

public interface IConversationQueueReadService
{
    Task<ConversationQueue?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
}
