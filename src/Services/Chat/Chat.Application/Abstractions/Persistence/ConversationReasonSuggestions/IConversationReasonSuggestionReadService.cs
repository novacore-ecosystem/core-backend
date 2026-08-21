namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationReasonSuggestions;

public interface IConversationReasonSuggestionReadService
{
    Task<ConversationReasonSuggestion?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);

    /// <summary>Active suggestions only, ordered by SortOrder - each item's translation for the given language (falls back to no translation if the language has none).</summary>
    Task<IReadOnlyList<ConversationReasonSuggestion>> GetActiveAsync(CancellationToken ct = default);
}
