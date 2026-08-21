using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationResponsibilityHistories;
using NovaCore.Chat.Persistence.Contexts.ConversationResponsibilityHistories.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationResponsibilityHistories.Write;

public sealed class ConversationResponsibilityHistoryWriteService(
    IConversationResponsibilityHistoryRepository historyRepo,
    IUnitOfWork unitOfWork) : IConversationResponsibilityHistoryWriteService
{
    public async Task CreateAsync(ConversationResponsibilityHistory history, CancellationToken ct = default)
    {
        await historyRepo.AddAsync(history, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await historyRepo.DeleteByIdAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
