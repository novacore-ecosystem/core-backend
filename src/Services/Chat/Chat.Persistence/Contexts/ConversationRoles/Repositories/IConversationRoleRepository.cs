using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Chat.Persistence.Contexts.ConversationRoles.Repositories;

public interface IConversationRoleRepository : IRepository<ConversationRole, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
