namespace NovaCore.Chat.Application.Abstractions.Persistence.Conversations;

public interface IConversationReadService
{
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
}
