using NovaCore.Auth.Domain.Entities.Permissions;

namespace NovaCore.Auth.Domain.Entities.Roles;

/// <summary>
/// Owned child of Role, referenced back via Role.Permissions - the many-to-many mapping of
/// roles to the permission definitions they grant.
/// </summary>
public sealed class RolePermission : BaseEntity, ITenantEntity
{
    public Guid RoleId { get; init; }
    public Role Role { get; init; } = default!;
    public Guid PermissionDefinitionId { get; init; }
    public PermissionDefinition PermissionDefinition { get; init; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private RolePermission() { }

    internal static RolePermission Create(Guid roleId, Guid permissionDefinitionId)
    {
        return new RolePermission
        {
            RoleId = roleId,
            PermissionDefinitionId = permissionDefinitionId,
        };
    }
}
