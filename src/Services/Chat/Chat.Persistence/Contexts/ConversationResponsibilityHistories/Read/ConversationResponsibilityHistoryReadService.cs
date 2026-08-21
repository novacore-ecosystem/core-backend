using NovaCore.Chat.Application.Abstractions.Persistence.ConversationResponsibilityHistories;
using NovaCore.Chat.Persistence.Contexts.ConversationResponsibilityHistories.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationResponsibilityHistories.Read;

public sealed class ConversationResponsibilityHistoryReadService(IConversationResponsibilityHistoryRepository historyRepo) : IConversationResponsibilityHistoryReadService
{
    public async Task<ConversationResponsibilityHistory?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await historyRepo.GetByIdAsync(id, ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await historyRepo.ExistsByIdAsync(id, ct);
    }
}
