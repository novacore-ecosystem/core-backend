namespace NovaCore.Chat.Application.Features.Conversations.Queries.GetConversationStatus;

/// <summary>
/// Minimal recovery-check payload for a client that persisted a conversation id locally and wants
/// to know whether it can still be resumed, without pulling the full GetConversation payload -
/// see GetConversationStatusHandler.
/// </summary>
public sealed record GetConversationStatusQuery(Guid ConversationId) : IQuery<GetConversationStatusResponse>;

public sealed record GetConversationStatusResponse(
    Guid Id,
    ConversationStatus Status,
    string? Title,
    Guid? AssignedUserId,
    DateTime LastActivityAt);
