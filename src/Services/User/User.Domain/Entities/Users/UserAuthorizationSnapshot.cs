namespace NovaCore.User.Domain.Entities.Users;

/// <summary>
/// Owned 1:1 read model caching this User's effective, Auth-derived security permission set
/// (e.g. "system:root", "product:manage") - distinct from UserPermissionSnapshot, which merges
/// this service's own independent, business-segmentation UserRole/PermissionCollection concept
/// (see docs/services/user-service.md, "Denormalized Roles" - the two "Roles"/"permissions"
/// concepts share vocabulary but nothing else). Rebuild only stores an already-merged array handed
/// to it directly by AccountEffectivePermissionsChangedIntegrationEvent - Auth is the source of
/// truth and computes the merge itself; User never queries Auth's Role/RolePermission graph.
/// </summary>
public sealed class UserAuthorizationSnapshot : BaseEntity, ITenantEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public IReadOnlyList<string> Permissions { get; private set; } = [];
    public int Version { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserAuthorizationSnapshot() { }

    public static UserAuthorizationSnapshot Create(Guid userId)
    {
        return new UserAuthorizationSnapshot
        {
            UserId = userId,
            Permissions = [],
            Version = 0,
        };
    }

    public void Rebuild(IReadOnlyList<string> permissions)
    {
        Permissions = permissions;
        Version++;
        Touch();
    }
}
