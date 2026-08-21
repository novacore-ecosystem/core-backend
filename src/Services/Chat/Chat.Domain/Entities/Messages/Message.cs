namespace NovaCore.Chat.Domain.Entities.Messages;

/// <summary>
/// Root aggregate for a single chat message. Related to Conversation by ConversationId only (no
/// Conversation.Messages navigation - unbounded per-conversation volume, same "independently
/// constructible by id" pattern Promotion.Domain uses for its own unbounded children). Owns
/// Attachments/References/Reactions/Mentions directly (bounded per message); does not own
/// MessageDelivery (unbounded by participant count, see Entities/Deliveries/MessageDelivery.cs).
/// </summary>
public sealed class Message : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid ConversationId { get; private set; }
    public Guid? SenderUserId { get; private set; }
    public MessageSenderType SenderType { get; private set; }
    public string ClientMessageId { get; private set; } = string.Empty;
    public long Sequence { get; private set; }
    public MessageType Type { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public MessageFormat Format { get; private set; }
    public MessageStatus Status { get; private set; }
    public DateTime? EditedAt { get; private set; }
    public DateTime? RecalledAt { get; private set; }
    public ChatMetadata? Metadata { get; private set; }

    public ICollection<MessageAttachment> Attachments { get; private set; } = [];
    public ICollection<MessageReference> References { get; private set; } = [];
    public ICollection<MessageReaction> Reactions { get; private set; } = [];
    public ICollection<MessageMention> Mentions { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private Message() { }

    public static Message Create(
        Guid conversationId,
        MessageSenderType senderType,
        string clientMessageId,
        long sequence,
        MessageType type,
        string content,
        Guid? senderUserId = null,
        MessageFormat format = MessageFormat.PlainText,
        ChatMetadata? metadata = null)
    {
        ValidateClientMessageId(clientMessageId);

        return new Message
        {
            Id = Guid.CreateVersion7(),
            ConversationId = conversationId,
            SenderUserId = senderUserId,
            SenderType = senderType,
            ClientMessageId = clientMessageId,
            Sequence = sequence,
            Type = type,
            Content = content,
            Format = format,
            Status = MessageStatus.Active,
            Metadata = metadata,
        };
    }
    #endregion

    #region Content & lifecycle
    /// <summary>Whether editing is still within the configured edit window is an Application-layer concern (spec section 41) - this only enforces the message's own state.</summary>
    public void Edit(string content)
    {
        EnsureActive();

        Content = content;
        EditedAt = DateTime.UtcNow;
    }

    public void Recall()
    {
        EnsureActive();

        Status = MessageStatus.Recalled;
        RecalledAt = DateTime.UtcNow;
    }

    private void EnsureActive()
    {
        if (Status != MessageStatus.Active)
            throw ExceptionFactory.InvalidStatus($"Cannot modify a message in {Status} status.");
    }

    public static bool IsValidClientMessageId(string? clientMessageId) => !string.IsNullOrWhiteSpace(clientMessageId);

    private static void ValidateClientMessageId(string clientMessageId)
    {
        if (!IsValidClientMessageId(clientMessageId))
            throw ExceptionFactory.RequiredField("Client message id cannot be empty.");
    }
    #endregion

    #region Attachments
    public void AddAttachment(
        AttachmentType type,
        string fileName,
        string contentType,
        long size,
        string storageKey,
        int? width = null,
        int? height = null,
        TimeSpan? duration = null,
        ChatMetadata? metadata = null)
    {
        Attachments.Add(MessageAttachment.Create(Id, type, fileName, contentType, size, storageKey, width, height, duration, metadata));
    }

    public void RemoveAttachment(Guid attachmentId)
    {
        var attachment = Attachments.FirstOrDefault(a => a.Id == attachmentId)
            ?? throw ExceptionFactory.EntityNotFound<MessageAttachment>(attachmentId);

        Attachments.Remove(attachment);
    }
    #endregion

    #region References
    public void AddReference(Guid referencedMessageId, MessageReferenceType type, ChatMetadata? metadata = null)
    {
        if (referencedMessageId == Id)
            throw ExceptionFactory.InvalidState("A message cannot reference itself.");

        References.Add(MessageReference.Create(Id, referencedMessageId, type, metadata));
    }
    #endregion

    #region Mentions
    public void AddMention(Guid userId, TextSpan span, MentionType mentionType)
    {
        Mentions.Add(MessageMention.Create(Id, userId, span, mentionType));
    }
    #endregion

    #region Reactions
    /// <summary>A user has at most one active reaction per message - adding a new one changes the existing row rather than creating a second.</summary>
    public void AddReaction(Guid userId, ReactionType reactionType)
    {
        var existing = Reactions.FirstOrDefault(r => r.UserId == userId);
        if (existing is not null)
        {
            existing.Change(reactionType);
            return;
        }

        Reactions.Add(MessageReaction.Create(Id, userId, reactionType));
    }

    public void RemoveReaction(Guid userId)
    {
        var reaction = Reactions.FirstOrDefault(r => r.UserId == userId);
        if (reaction is null)
            return;

        Reactions.Remove(reaction);
    }
    #endregion
}
