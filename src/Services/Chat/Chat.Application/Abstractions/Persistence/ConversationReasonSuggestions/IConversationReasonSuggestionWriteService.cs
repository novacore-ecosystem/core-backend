using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationReasonSuggestions;

public interface IConversationReasonSuggestionWriteService
{
    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task CreateAsync(ConversationReasonSuggestion suggestion, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task TranslateAsync(Guid id, LanguageCode languageCode, string text, CancellationToken ct = default);
}
