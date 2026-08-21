namespace NovaCore.Chat.Application.Features.Conversations.Commands.CreateConversation;

public sealed record CreateConversationCommand(
    ConversationType Type,
    ConversationLifecycle Lifecycle,
    string? Title = null,
    string? Description = null,
    string? Avatar = null,
    ConversationPriority Priority = ConversationPriority.Normal) : ICommand<CreateConversationResponse>;

public sealed record CreateConversationResponse(Guid ConversationId);
