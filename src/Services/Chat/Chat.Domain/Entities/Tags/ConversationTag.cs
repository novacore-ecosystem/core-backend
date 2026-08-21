using NovaCore.Chat.Domain.Entities.Conversations;

namespace NovaCore.Chat.Domain.Entities.Tags;

/// <summary>Mapping between a Conversation and a Tag - composite key (ConversationId, TagId).</summary>
public sealed class ConversationTag : BaseEntity, ITenantEntity
{
    public Guid ConversationId { get; private set; }
    public Conversation Conversation { get; private set; } = default!;
    public Guid TagId { get; private set; }
    public Tag Tag { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ConversationTag() { }

    /// <summary>Only Conversation may construct a ConversationTag - see Conversation.AssignTag.</summary>
    internal static ConversationTag Create(Guid conversationId, Guid tagId)
    {
        return new ConversationTag
        {
            ConversationId = conversationId,
            TagId = tagId,
        };
    }
}
