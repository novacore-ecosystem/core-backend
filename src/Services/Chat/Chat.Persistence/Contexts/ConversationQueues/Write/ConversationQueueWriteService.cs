using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationQueues;
using NovaCore.Chat.Persistence.Contexts.ConversationQueues.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationQueues.Write;

public sealed class ConversationQueueWriteService(
    IConversationQueueRepository queueRepo,
    IUnitOfWork unitOfWork) : IConversationQueueWriteService
{
    public async Task CreateAsync(ConversationQueue queue, CancellationToken ct = default)
    {
        await queueRepo.AddAsync(queue, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await queueRepo.DeleteByIdAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
