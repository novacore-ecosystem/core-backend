namespace NovaCore.Chat.Domain.Entities.Messages;

/// <summary>Reply/quote/forward pointer to another message in the same conversation. ReferencedMessageId is a plain id - resolving the target message is an Application-layer concern (spec section 24), so no navigation property is exposed here.</summary>
public sealed class MessageReference : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid MessageId { get; private set; }
    public Message Message { get; private set; } = default!;
    public Guid ReferencedMessageId { get; private set; }
    public MessageReferenceType Type { get; private set; }
    public ChatMetadata? Metadata { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private MessageReference() { }

    /// <summary>Only Message may construct a MessageReference - see Message.AddReference.</summary>
    internal static MessageReference Create(Guid messageId, Guid referencedMessageId, MessageReferenceType type, ChatMetadata? metadata)
    {
        return new MessageReference
        {
            Id = Guid.CreateVersion7(),
            MessageId = messageId,
            ReferencedMessageId = referencedMessageId,
            Type = type,
            Metadata = metadata,
        };
    }
}
