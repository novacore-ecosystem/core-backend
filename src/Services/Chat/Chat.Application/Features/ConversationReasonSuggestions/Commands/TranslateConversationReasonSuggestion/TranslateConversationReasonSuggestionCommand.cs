namespace NovaCore.Chat.Application.Features.ConversationReasonSuggestions.Commands.TranslateConversationReasonSuggestion;

public sealed record TranslateConversationReasonSuggestionCommand(
    Guid ConversationReasonSuggestionId,
    string LanguageCode,
    string Text) : ICommand;
