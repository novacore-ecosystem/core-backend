namespace NovaCore.Chat.Domain.Entities.Notifications;

/// <summary>Per-(Conversation, User) notification configuration - composite key. No durable per-message notification record (spec section 36).</summary>
public sealed class ConversationNotificationPreference : BaseEntity, ITenantEntity
{
    public Guid ConversationId { get; private set; }
    public Guid UserId { get; private set; }
    public ConversationNotificationMode Mode { get; private set; }
    public DateTime? MutedUntil { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ConversationNotificationPreference() { }

    public static ConversationNotificationPreference Create(
        Guid conversationId,
        Guid userId,
        ConversationNotificationMode mode = ConversationNotificationMode.All)
    {
        return new ConversationNotificationPreference
        {
            ConversationId = conversationId,
            UserId = userId,
            Mode = mode,
        };
    }

    public void ChangeMode(ConversationNotificationMode mode)
    {
        Mode = mode;
    }

    public void MuteUntil(DateTime? mutedUntil)
    {
        MutedUntil = mutedUntil;
    }
}
