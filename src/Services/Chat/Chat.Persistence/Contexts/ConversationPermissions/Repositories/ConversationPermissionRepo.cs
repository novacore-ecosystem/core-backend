using NovaCore.Chat.Persistence.Contexts;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.ConversationPermissions.Repositories;

public sealed class ConversationPermissionRepo(ChatDbContext dbContext)
    : ChatBaseRepository<ConversationPermission, Guid>(dbContext), IConversationPermissionRepository
{
}
