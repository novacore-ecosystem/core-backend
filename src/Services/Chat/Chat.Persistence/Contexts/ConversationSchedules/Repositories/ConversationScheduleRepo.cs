using NovaCore.Chat.Persistence.Contexts;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.ConversationSchedules.Repositories;

public sealed class ConversationScheduleRepo(ChatDbContext dbContext)
    : ChatBaseRepository<ConversationSchedule, Guid>(dbContext), IConversationScheduleRepository
{
}
