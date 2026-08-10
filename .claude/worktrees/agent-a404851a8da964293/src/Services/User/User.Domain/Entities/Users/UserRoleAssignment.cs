using NovaCore.User.Domain.Entities.Roles;

namespace NovaCore.User.Domain.Entities.Users;

/// <summary>
/// Owned child of User recording one grant of an independent UserRole - User never holds a Role
/// object directly, only this reference plus grant metadata (who/when/until). Kept as history:
/// revoking or expiring a grant transitions Status rather than deleting the row, so "who had
/// access to what, and when" remains reconstructable. Carries no Role data (name/permissions) -
/// that would duplicate the Role aggregate.
/// </summary>
public sealed class UserRoleAssignment : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public Guid RoleId { get; private set; }
    public UserRole Role { get; private set; } = default!;
    public DateTime AssignedAt { get; private set; }
    public Guid? AssignedBy { get; private set; }
    public DateTime? ExpiredAt { get; private set; }
    public UserRoleAssignmentStatus Status { get; private set; } = UserRoleAssignmentStatus.Active;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserRoleAssignment() { }

    internal static UserRoleAssignment Create(Guid userId, Guid roleId, Guid? assignedBy = null, DateTime? expiredAt = null)
    {
        return new UserRoleAssignment
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = assignedBy,
            ExpiredAt = expiredAt,
            Status = UserRoleAssignmentStatus.Active,
        };
    }

    /// <summary>True only for a grant that is both administratively Active and, if it carries an
    /// ExpiredAt, not yet past it - the predicate the permission-merge process should filter on.</summary>
    public bool IsEffective
        => Status == UserRoleAssignmentStatus.Active && (ExpiredAt is null || ExpiredAt > DateTime.UtcNow);

    // ============================================================================
    // Details & lifecycle
    // The Active -> Expired/Revoked state transitions. Each is only valid from
    // Active, so a decided grant can't silently flip to a different outcome.
    // ============================================================================

    #region Details & lifecycle

    public void Revoke()
    {
        EnsureActive();

        Status = UserRoleAssignmentStatus.Revoked;
    }

    public void Expire()
    {
        EnsureActive();

        Status = UserRoleAssignmentStatus.Expired;
    }

    private void EnsureActive()
    {
        if (Status != UserRoleAssignmentStatus.Active)
            throw ExceptionFactory.InvalidState("Only an active role assignment can transition state.");
    }

    #endregion
}
