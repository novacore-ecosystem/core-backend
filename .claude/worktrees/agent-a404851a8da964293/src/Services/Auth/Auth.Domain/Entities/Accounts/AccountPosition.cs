using NovaCore.Auth.Domain.Entities.Positions;

namespace NovaCore.Auth.Domain.Entities.Accounts;

/// <summary>
/// Owned child of Account recording one grant of an organizational Position - the primary
/// authorization-assignment path (see Account.AssignPosition). Kept as history: revoking or
/// expiring a grant transitions Status rather than deleting the row, so a personnel change
/// (Employee A replaced by Employee B) never loses "who held what, and when." Unlike PositionRole/
/// RolePermission, this mapping has its own lifecycle (AssignedAt/RevokedAt/Status) and therefore
/// keeps a surrogate Id, per the many-to-many exception in domain-coding-conventions.md Rule 4.
/// </summary>
public sealed class AccountPosition : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid AccountId { get; private set; }
    public Account Account { get; private set; } = default!;
    public Guid PositionId { get; private set; }
    public Position Position { get; private set; } = default!;
    public DateTime AssignedAt { get; private set; }
    public Guid? AssignedBy { get; private set; }
    public DateTime? ExpiredAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public AccountPositionStatus Status { get; private set; } = AccountPositionStatus.Active;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private AccountPosition() { }

    internal static AccountPosition Create(
        Guid accountId,
        Guid positionId,
        Guid? assignedBy = null,
        DateTime? expiredAt = null)
    {
        return new AccountPosition
        {
            Id = Guid.CreateVersion7(),
            AccountId = accountId,
            PositionId = positionId,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = assignedBy,
            ExpiredAt = expiredAt,
            Status = AccountPositionStatus.Active,
        };
    }

    /// <summary>
    /// True only for a grant that is both administratively Active and, if it carries an
    /// ExpiredAt, not yet past it - the predicate permission resolution should filter on.
    /// </summary>
    public bool IsEffective
        => Status == AccountPositionStatus.Active && (ExpiredAt is null || ExpiredAt > DateTime.UtcNow);

    // ============================================================================
    // Details & lifecycle
    // The Active -> Expired/Revoked state transitions. Each is only valid from
    // Active, so a decided grant can't silently flip to a different outcome.
    // ============================================================================

    #region Details & lifecycle

    public void Revoke()
    {
        EnsureActive();

        Status = AccountPositionStatus.Revoked;
        RevokedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        EnsureActive();

        Status = AccountPositionStatus.Expired;
    }

    private void EnsureActive()
    {
        if (Status != AccountPositionStatus.Active)
            throw ExceptionFactory.InvalidState("Only an active position assignment can transition state.");
    }

    #endregion
}
