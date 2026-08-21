using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationTasks;
using NovaCore.Chat.Persistence.Contexts.ConversationTasks.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationTasks.Write;

public sealed class ConversationTaskWriteService(
    IConversationTaskRepository taskRepo,
    IUnitOfWork unitOfWork) : IConversationTaskWriteService
{
    public async Task CreateAsync(ConversationTask task, CancellationToken ct = default)
    {
        await taskRepo.AddAsync(task, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await taskRepo.DeleteByIdAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
