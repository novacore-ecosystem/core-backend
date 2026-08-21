using NovaCore.Chat.Application.Abstractions.Persistence.ConversationTasks;
using NovaCore.Chat.Persistence.Contexts.ConversationTasks.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationTasks.Read;

public sealed class ConversationTaskReadService(IConversationTaskRepository taskRepo) : IConversationTaskReadService
{
    public async Task<ConversationTask?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await taskRepo.GetByIdAsync(id, ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await taskRepo.ExistsByIdAsync(id, ct);
    }
}
