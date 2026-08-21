namespace NovaCore.Chat.Domain.Entities.ReadStates;

/// <summary>
/// Compact read-position per (Conversation, User) - composite key, one row that moves forward,
/// not one durable row per read message (spec section 28). Independently constructible, not owned
/// by Conversation.
/// </summary>
public sealed class ConversationReadState : BaseEntity, ITenantEntity
{
    public Guid ConversationId { get; private set; }
    public Guid UserId { get; private set; }
    public long LastReadSequence { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ConversationReadState() { }

    public static ConversationReadState Create(Guid conversationId, Guid userId, long lastReadSequence = 0)
    {
        return new ConversationReadState
        {
            ConversationId = conversationId,
            UserId = userId,
            LastReadSequence = lastReadSequence,
        };
    }

    public void AdvanceTo(long sequence)
    {
        if (sequence > LastReadSequence)
            LastReadSequence = sequence;
    }
}
