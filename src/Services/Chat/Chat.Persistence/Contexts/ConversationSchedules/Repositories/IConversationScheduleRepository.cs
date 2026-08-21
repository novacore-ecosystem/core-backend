using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Chat.Persistence.Contexts.ConversationSchedules.Repositories;

public interface IConversationScheduleRepository : IRepository<ConversationSchedule, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
