namespace NovaCore.Chat.Application.Features.Conversations.Commands.RateConversation;

public sealed record RateConversationCommand(
    Guid ConversationId,
    byte Stars,
    string? Review = null) : ICommand<RateConversationResponse>;

public sealed record RateConversationResponse(Guid RatingId);
