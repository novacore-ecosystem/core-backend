namespace NovaCore.Chat.Domain.Entities.Messages;

/// <summary>An @mention within a message's Content - Span anchors it to the highlighted text. Does not duplicate the mentioned user's display name (spec section 26).</summary>
public sealed class MessageMention : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid MessageId { get; private set; }
    public Message Message { get; private set; } = default!;
    public Guid UserId { get; private set; }
    public TextSpan Span { get; private set; } = default!;
    public MentionType MentionType { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private MessageMention() { }

    /// <summary>Only Message may construct a MessageMention - see Message.AddMention.</summary>
    internal static MessageMention Create(Guid messageId, Guid userId, TextSpan span, MentionType mentionType)
    {
        return new MessageMention
        {
            Id = Guid.CreateVersion7(),
            MessageId = messageId,
            UserId = userId,
            Span = span,
            MentionType = mentionType,
        };
    }
}
