using NovaCore.Chat.Domain.Entities.Conversations;

namespace NovaCore.Chat.Domain.Entities.Contacts;

/// <summary>Mapping between a Conversation and the business Contact identity it was started through - one row per conversation (see Conversation.AssignContact).</summary>
public sealed class ConversationContact : BaseEntity, ITenantEntity
{
    public Guid ConversationId { get; private set; }
    public Conversation Conversation { get; private set; } = default!;
    public Guid ContactId { get; private set; }
    public Contact Contact { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ConversationContact() { }

    /// <summary>Only Conversation may construct a ConversationContact - see Conversation.AssignContact.</summary>
    internal static ConversationContact Create(Guid conversationId, Guid contactId)
    {
        return new ConversationContact
        {
            ConversationId = conversationId,
            ContactId = contactId,
        };
    }
}
