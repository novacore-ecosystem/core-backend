using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Chat.Persistence.Contexts.ConversationPermissions.Repositories;

public interface IConversationPermissionRepository : IRepository<ConversationPermission, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
