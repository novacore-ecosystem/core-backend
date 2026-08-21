namespace NovaCore.Chat.Application.Features.ConversationReasonSuggestions.Commands.CreateConversationReasonSuggestion;

public sealed record CreateConversationReasonSuggestionCommand(
    string Code,
    int SortOrder = 0) : ICommand<CreateConversationReasonSuggestionResponse>;

public sealed record CreateConversationReasonSuggestionResponse(Guid Id);
