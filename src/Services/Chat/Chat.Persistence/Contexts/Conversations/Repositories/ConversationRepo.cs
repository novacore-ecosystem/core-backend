using NovaCore.Chat.Persistence.Contexts;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.Conversations.Repositories;

public sealed class ConversationRepo(ChatDbContext dbContext)
    : ChatBaseRepository<Conversation, Guid>(dbContext), IConversationRepository
{
}
