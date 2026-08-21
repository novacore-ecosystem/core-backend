using NovaCore.Chat.Persistence.Contexts;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.ConversationRoles.Repositories;

public sealed class ConversationRoleRepo(ChatDbContext dbContext)
    : ChatBaseRepository<ConversationRole, Guid>(dbContext), IConversationRoleRepository
{
}
