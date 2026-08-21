namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationTasks;

public interface IConversationTaskReadService
{
    Task<ConversationTask?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
}
