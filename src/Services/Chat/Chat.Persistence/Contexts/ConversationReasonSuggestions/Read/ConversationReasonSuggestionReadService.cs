using NovaCore.Chat.Application.Abstractions.Persistence.ConversationReasonSuggestions;
using NovaCore.Chat.Persistence.Contexts.ConversationReasonSuggestions.Repositories;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.ConversationReasonSuggestions.Read;

public sealed class ConversationReasonSuggestionReadService(
    IConversationReasonSuggestionRepository suggestionRepo,
    ChatDbContext dbContext) : IConversationReasonSuggestionReadService
{
    public async Task<ConversationReasonSuggestion?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await suggestionRepo.GetByIdAsync(id, query => query.Include(s => s.Translations), ct);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        var normalized = EntityCode.Create(code);
        return await suggestionRepo.ExistsAsync(s => s.Code == normalized, ct);
    }

    public async Task<IReadOnlyList<ConversationReasonSuggestion>> GetActiveAsync(CancellationToken ct = default)
    {
        return await dbContext.ConversationReasonSuggestions
            .AsNoTracking()
            .Include(s => s.Translations)
            .Where(s => s.Status == ConversationReasonSuggestionStatus.Active)
            .OrderBy(s => s.SortOrder)
            .ToListAsync(ct);
    }
}
