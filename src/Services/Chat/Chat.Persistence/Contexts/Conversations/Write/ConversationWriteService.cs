using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Chat.Application.Abstractions.Persistence.Conversations;
using NovaCore.Chat.Persistence.Contexts.Conversations.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.Conversations.Write;

public sealed class ConversationWriteService(
    IConversationRepository conversationRepo,
    IUnitOfWork unitOfWork) : IConversationWriteService
{
    public async Task CreateAsync(Conversation conversation, CancellationToken ct = default)
    {
        await conversationRepo.AddAsync(conversation, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await conversationRepo.DeleteByIdAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task CloseAsync(Guid id, CancellationToken ct = default)
    {
        await conversationRepo.UpdateAsync(id, c => c.Close(), ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task OpenAsync(Guid id, CancellationToken ct = default)
    {
        await conversationRepo.UpdateAsync(id, c => c.Open(), ct);
    }
}
