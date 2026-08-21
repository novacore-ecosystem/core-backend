using NovaCore.Chat.Application.Abstractions.Persistence.Conversations;
using NovaCore.Chat.Persistence.Contexts.Conversations.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.Conversations.Read;

public sealed class ConversationReadService(IConversationRepository conversationRepo) : IConversationReadService
{
    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await conversationRepo.GetByIdAsync(
            id,
            query => query
                .Include(c => c.ContactMappings)
                .Include(c => c.Participants)
                .Include(c => c.TagMappings)
                .Include(c => c.Notes)
                .Include(c => c.PinnedMessages),
            ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await conversationRepo.ExistsByIdAsync(id, ct);
    }
}
