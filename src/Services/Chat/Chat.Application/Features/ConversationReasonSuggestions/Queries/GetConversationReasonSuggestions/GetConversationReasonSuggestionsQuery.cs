namespace NovaCore.Chat.Application.Features.ConversationReasonSuggestions.Queries.GetConversationReasonSuggestions;

public sealed record GetConversationReasonSuggestionsQuery(string LanguageCode)
    : IQuery<IReadOnlyList<ConversationReasonSuggestionDto>>;

public sealed record ConversationReasonSuggestionDto(Guid Id, string Code, string Text);
