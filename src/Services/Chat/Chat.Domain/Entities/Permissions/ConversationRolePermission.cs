using NovaCore.Chat.Domain.Entities.Roles;

namespace NovaCore.Chat.Domain.Entities.Permissions;

/// <summary>Mapping granting a ConversationPermission to a ConversationRole - composite key (RoleId, PermissionId).</summary>
public sealed class ConversationRolePermission : BaseEntity, ITenantEntity
{
    public Guid RoleId { get; private set; }
    public ConversationRole Role { get; private set; } = default!;
    public Guid PermissionId { get; private set; }
    public ConversationPermission Permission { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ConversationRolePermission() { }

    public static ConversationRolePermission Create(Guid roleId, Guid permissionId)
    {
        return new ConversationRolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
        };
    }
}
