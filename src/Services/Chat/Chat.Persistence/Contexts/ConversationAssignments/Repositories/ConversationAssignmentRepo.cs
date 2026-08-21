using NovaCore.Chat.Persistence.Contexts;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.ConversationAssignments.Repositories;

public sealed class ConversationAssignmentRepo(ChatDbContext dbContext)
    : ChatBaseRepository<ConversationAssignment, Guid>(dbContext), IConversationAssignmentRepository
{
}
