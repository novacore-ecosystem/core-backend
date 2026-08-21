namespace NovaCore.Chat.Application.Features.Conversations.Commands.StartGuestConversation;

/// <summary>
/// Spec section 39's guest flow: a guest submits basic contact information, a Contact (UserId
/// null) is created/resolved, a Session conversation is opened and enters the queue, and the
/// caller gets back a short-lived access token so their browser can reconnect (REST + ChatHub)
/// as that same guest identity - see StartGuestConversationHandler and
/// Chat.Infrastructure's GuestTokenGenerator.
/// </summary>
public sealed record StartGuestConversationCommand(
    string DisplayName,
    string? Email = null,
    string? Phone = null) : ICommand<StartGuestConversationResponse>;

public sealed record StartGuestConversationResponse(
    Guid ContactId,
    Guid ConversationId,
    string AccessToken);
