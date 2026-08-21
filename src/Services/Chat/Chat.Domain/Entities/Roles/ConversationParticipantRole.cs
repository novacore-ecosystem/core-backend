namespace NovaCore.Chat.Domain.Entities.Roles;

/// <summary>Mapping granting a ConversationRole to one participant of one conversation - composite key (ConversationId, UserId, RoleId), per spec section 34.</summary>
public sealed class ConversationParticipantRole : BaseEntity, ITenantEntity
{
    public Guid ConversationId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public ConversationRole Role { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ConversationParticipantRole() { }

    public static ConversationParticipantRole Create(Guid conversationId, Guid userId, Guid roleId)
    {
        return new ConversationParticipantRole
        {
            ConversationId = conversationId,
            UserId = userId,
            RoleId = roleId,
        };
    }
}
