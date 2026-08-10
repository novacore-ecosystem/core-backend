namespace NovaCore.User.Domain.Entities.Users;

/// <summary>
/// Owned 1:1 read model caching this User's merged, deduplicated permission set across every
/// effective UserRoleAssignment - built to make authorization checks a single-row lookup instead
/// of a fan-out across Roles. The merge itself (reading each assigned UserRole's
/// PermissionCollection and combining via PermissionCollection.Merge) is cross-aggregate and
/// therefore an Application-layer concern, not something User computes internally; Rebuild only
/// stores an already-merged result handed to it. Deliberately not IAuditable - this is a derived
/// cache, not user-authored business data. UpdatedAt is the inherited BaseEntity property,
/// touched on every Rebuild rather than duplicated as a separate field.
/// </summary>
public sealed class UserPermissionSnapshot : BaseEntity, ITenantEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public PermissionCollection Permissions { get; private set; } = PermissionCollection.Empty;
    public int Version { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserPermissionSnapshot() { }

    public static UserPermissionSnapshot Create(Guid userId)
    {
        return new UserPermissionSnapshot
        {
            UserId = userId,
            Permissions = PermissionCollection.Empty,
            Version = 0,
        };
    }

    /// <summary>Replaces the cached permission set wholesale and bumps Version - called by the
    /// asynchronous rebuild process (see RolePermissionChanged/UserRoleAssigned/UserRoleRemoved),
    /// never synchronously from a User role-assignment command.</summary>
    public void Rebuild(PermissionCollection mergedPermissions)
    {
        Permissions = mergedPermissions;
        Version++;
        Touch();
    }
}
