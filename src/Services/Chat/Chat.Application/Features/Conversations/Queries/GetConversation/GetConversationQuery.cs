namespace NovaCore.Chat.Application.Features.Conversations.Queries.GetConversation;

public sealed record GetConversationQuery(Guid ConversationId) : IQuery<GetConversationResponse>;

public sealed record GetConversationResponse(
    Guid Id,
    ConversationType Type,
    ConversationLifecycle Lifecycle,
    ConversationStatus Status,
    string? Title,
    string? Description,
    string? Avatar,
    string? Reason,
    ConversationPriority Priority,
    DateTime? ClosedAt,
    DateTime LastActivityAt,
    long LastMessageSequence,
    DateTime CreatedAt,
    DateTime UpdatedAt);
