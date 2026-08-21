using NovaCore.Chat.Domain.Entities.Conversations;

namespace NovaCore.Chat.Domain.Entities.Pins;

/// <summary>A message pinned to a conversation - composite key (ConversationId, MessageId), one row per pin.</summary>
public sealed class ConversationPinnedMessage : BaseEntity, ITenantEntity
{
    public Guid ConversationId { get; private set; }
    public Conversation Conversation { get; private set; } = default!;
    public Guid MessageId { get; private set; }
    public Guid PinnedByUserId { get; private set; }
    public DateTime PinnedAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ConversationPinnedMessage() { }

    /// <summary>Only Conversation may construct a ConversationPinnedMessage - see Conversation.PinMessage.</summary>
    internal static ConversationPinnedMessage Create(Guid conversationId, Guid messageId, Guid pinnedByUserId)
    {
        return new ConversationPinnedMessage
        {
            ConversationId = conversationId,
            MessageId = messageId,
            PinnedByUserId = pinnedByUserId,
            PinnedAt = DateTime.UtcNow,
        };
    }
}
