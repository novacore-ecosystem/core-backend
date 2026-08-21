namespace NovaCore.Chat.Domain.Entities.Messages;

/// <summary>A user's single active reaction on a message - composite key (MessageId, UserId) enforces at most one reaction per user (spec section 25). No reaction history.</summary>
public sealed class MessageReaction : BaseEntity, ITenantEntity
{
    public Guid MessageId { get; private set; }
    public Message Message { get; private set; } = default!;
    public Guid UserId { get; private set; }
    public ReactionType ReactionType { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private MessageReaction() { }

    /// <summary>Only Message may construct a MessageReaction - see Message.AddReaction.</summary>
    internal static MessageReaction Create(Guid messageId, Guid userId, ReactionType reactionType)
    {
        return new MessageReaction
        {
            MessageId = messageId,
            UserId = userId,
            ReactionType = reactionType,
        };
    }

    internal void Change(ReactionType reactionType)
    {
        ReactionType = reactionType;
    }
}
