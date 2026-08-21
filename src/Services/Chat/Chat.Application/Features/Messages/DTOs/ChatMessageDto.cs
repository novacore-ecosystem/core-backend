namespace NovaCore.Chat.Application.Features.Messages.DTOs;

/// <summary>Realtime push payload for a new message - deliberately smaller than GetMessage's future full response (no Attachments/References/Reactions/Mentions), since a hub push is a live notification, not a page load.</summary>
public sealed record ChatMessageDto(
    Guid Id,
    Guid ConversationId,
    Guid? SenderUserId,
    MessageSenderType SenderType,
    long Sequence,
    MessageType Type,
    string Content,
    MessageFormat Format,
    DateTime CreatedAt);
