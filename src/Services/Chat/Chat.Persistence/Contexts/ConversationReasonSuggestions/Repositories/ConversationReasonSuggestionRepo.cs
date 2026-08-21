using NovaCore.Chat.Persistence.Contexts;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.ConversationReasonSuggestions.Repositories;

public sealed class ConversationReasonSuggestionRepo(ChatDbContext dbContext)
    : ChatBaseRepository<ConversationReasonSuggestion, Guid>(dbContext), IConversationReasonSuggestionRepository
{
}
