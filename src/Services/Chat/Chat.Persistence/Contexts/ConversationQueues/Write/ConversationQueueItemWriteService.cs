using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationQueues;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.ConversationQueues.Write;

/// <summary>See ConversationQueueItemReadService's doc comment - same "no repository, ChatDbContext directly" reasoning.</summary>
public sealed class ConversationQueueItemWriteService(
    ChatDbContext dbContext,
    IUnitOfWork unitOfWork) : IConversationQueueItemWriteService
{
    public async Task EnqueueAsync(Guid queueId, Guid conversationId, int priority, CancellationToken ct = default)
    {
        dbContext.ConversationQueueItems.Add(ConversationQueueItem.Create(queueId, conversationId, priority));
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task MarkAssignedAsync(Guid conversationId, CancellationToken ct = default)
    {
        var item = await dbContext.ConversationQueueItems.FirstOrDefaultAsync(
            i => i.ConversationId == conversationId && i.Status == ConversationQueueItemStatus.Waiting, ct)
            ?? throw new NotFoundException(nameof(ConversationQueueItem), conversationId);

        item.MarkAssigned();
        await unitOfWork.SaveChangesAsync(ct);
    }
}
