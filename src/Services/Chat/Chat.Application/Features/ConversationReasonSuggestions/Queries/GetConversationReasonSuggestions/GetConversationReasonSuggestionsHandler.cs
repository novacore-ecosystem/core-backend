using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationReasonSuggestions;

namespace NovaCore.Chat.Application.Features.ConversationReasonSuggestions.Queries.GetConversationReasonSuggestions;

public sealed class GetConversationReasonSuggestionsHandler(IConversationReasonSuggestionReadService suggestionReadService)
    : IQueryHandler<GetConversationReasonSuggestionsQuery, IReadOnlyList<ConversationReasonSuggestionDto>>
{
    public async Task<IReadOnlyList<ConversationReasonSuggestionDto>> Handle(
        GetConversationReasonSuggestionsQuery request,
        CancellationToken ct = default)
    {
        var languageCode = LanguageCode.Create(request.LanguageCode);
        var suggestions = await suggestionReadService.GetActiveAsync(ct);

        // Only suggestions translated into the requested language are shown - a suggestion with no
        // translation there has nothing to display.
        return [.. suggestions
            .Select(s => (Suggestion: s, Translation: s.Translations.FirstOrDefault(t => t.LanguageCode == languageCode)))
            .Where(x => x.Translation is not null)
            .Select(x => new ConversationReasonSuggestionDto(x.Suggestion.Id, x.Suggestion.Code.Value, x.Translation!.Text))];
    }
}
