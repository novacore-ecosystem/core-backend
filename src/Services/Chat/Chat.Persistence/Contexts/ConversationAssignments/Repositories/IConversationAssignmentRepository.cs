using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Chat.Persistence.Contexts.ConversationAssignments.Repositories;

public interface IConversationAssignmentRepository : IRepository<ConversationAssignment, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
