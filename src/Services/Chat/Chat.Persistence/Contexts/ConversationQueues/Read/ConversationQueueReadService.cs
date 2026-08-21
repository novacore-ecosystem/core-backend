using NovaCore.Chat.Application.Abstractions.Persistence.ConversationQueues;
using NovaCore.Chat.Persistence.Contexts.ConversationQueues.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationQueues.Read;

public sealed class ConversationQueueReadService(IConversationQueueRepository queueRepo) : IConversationQueueReadService
{
    public async Task<ConversationQueue?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await queueRepo.GetByIdAsync(id, ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await queueRepo.ExistsByIdAsync(id, ct);
    }
}
