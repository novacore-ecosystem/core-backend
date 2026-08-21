namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationResponsibilityHistories;

public interface IConversationResponsibilityHistoryReadService
{
    Task<ConversationResponsibilityHistory?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
}
