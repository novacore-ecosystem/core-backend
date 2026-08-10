using Microsoft.AspNetCore.Identity;

using NovaCore.Auth.Domain.Entities.Positions;
using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.BuildingBlock.Domain.Attributes;
using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Auth.Domain.Entities.Accounts;

/// <summary>
/// Aggregate root of the Auth service - the JWT-claim authority for the platform. Holds identity
/// (via ASP.NET Core Identity), authentication state, and ownership of every account-scoped
/// security concern (positions, roles, permission cache, sessions, devices, MFA, external logins,
/// password history). Business-domain role/permission management is out of scope - that is
/// User.Domain's concern; Account only cares about what ends up in the token.
///
/// Position is the primary authorization-assignment unit (AssignPosition/RevokePosition) -
/// administrators grant an organizational responsibility, not a flat list of Roles, so a
/// personnel change only requires re-pointing the Position. Direct Role assignment
/// (AssignRole/RemoveRole) still exists for exceptional cases that don't map to any Position
/// (e.g. a one-off elevated grant), but is not the normal management path.
/// </summary>
public sealed class Account : IdentityUser<Guid>, IEntity, IAuditable
{
    public AccountStatus Status { get; private set; }
    public bool IsMfaEnabled { get; private set; }
    public int FailedLoginCount { get; private set; }

    public ICollection<AccountPosition> AccountPositions { get; private set; } = [];
    public ICollection<AccountRole> AccountRoles { get; private set; } = [];
    public ICollection<AccountPermission> Permissions { get; private set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];
    public ICollection<Session> Sessions { get; private set; } = [];
    public ICollection<Device> Devices { get; private set; } = [];
    public ICollection<MfaMethod> MfaMethods { get; private set; } = [];
    public ICollection<ExternalIdentity> ExternalIdentities { get; private set; } = [];
    public ICollection<PasswordHistory> PasswordHistories { get; private set; } = [];

    public DateTime CreatedAt { get; set; }

    [AuditIgnore]
    public DateTime UpdatedAt { get; set; }

    private Account() { }

    public static Account Create(
        string username,
        Email email,
        AccountStatus status = AccountStatus.Active)
        => Create(Guid.CreateVersion7(), username, email, status);

    /// <summary>
    /// Creates an Account with an explicit id. Used when the Account must share its
    /// identity with a UserProfile created first in the User service (admin/root-initiated
    /// user creation), rather than generating a fresh id (self-registration).
    /// </summary>
    public static Account Create(
        Guid id,
        string username,
        Email email,
        AccountStatus status = AccountStatus.Active)
    {
        return new Account
        {
            Id = id,
            UserName = username,
            Email = email.Value,
            EmailConfirmed = false,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void Track()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    // ============================================================================
    // Lifecycle
    // Status transitions for the account as a whole. Deleted/Suspended/Locked are
    // distinct terminal-ish states so lockout policy and admin action can be told
    // apart from a user-initiated deactivation.
    // ============================================================================

    #region Lifecycle

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
    }

    public void Activate()
    {
        Status = AccountStatus.Active;
    }

    public void Deactivate()
    {
        Status = AccountStatus.Inactive;
    }

    public void Lock()
    {
        Status = AccountStatus.Locked;
    }

    public void Unlock()
    {
        Status = Status == AccountStatus.Locked
            ? AccountStatus.Active
            : Status;

        FailedLoginCount = 0;
    }

    public void Suspend()
    {
        Status = AccountStatus.Suspended;
    }

    public void MarkDeleted()
    {
        Status = AccountStatus.Deleted;
    }

    #endregion

    // ============================================================================
    // Authentication
    // Password-change bookkeeping and failed-login tracking. Does not decide lockout
    // thresholds itself (that is an Application-layer policy) - only exposes the
    // raw counters and history recording.
    // ============================================================================

    #region Authentication

    public void RecordPasswordChange(string passwordHash)
    {
        PasswordHistories.Add(PasswordHistory.Record(Id, passwordHash));
        PasswordHash = passwordHash;
    }

    public void RegisterFailedLogin()
    {
        FailedLoginCount++;
    }

    public void ResetFailedLoginCount()
    {
        FailedLoginCount = 0;
    }

    #endregion

    // ============================================================================
    // Position
    // Manages the owned AccountPosition history - the primary authorization-assignment
    // path. Grants are never deleted, only transitioned (Revoke/Expire), so "who held
    // what, and when" survives every personnel change. An Account may hold several
    // Positions concurrently (e.g. covering two responsibilities at once).
    // ============================================================================

    #region Position

    public void AssignPosition(
        Position position,
        Guid? assignedBy = null,
        DateTime? expiredAt = null)
    {
        if (AccountPositions.Any(ap => ap.PositionId == position.Id && ap.IsEffective))
            throw ExceptionFactory.Duplicate("Account already has an active assignment for this position.");

        var accountPosition = AccountPosition.Create(
            Id,
            position.Id,
            assignedBy,
            expiredAt);
        AccountPositions.Add(accountPosition);
    }

    public void RevokePosition(Guid positionId)
    {
        var accountPosition = AccountPositions
            .FirstOrDefault(ap => ap.PositionId == positionId && ap.IsEffective);
        if (accountPosition is null)
            return;

        accountPosition.Revoke();
    }

    #endregion

    // ============================================================================
    // Role & Permission
    // AssignRole/RemoveRole is the exceptional, direct-assignment path for grants
    // that don't map to any Position - normal management goes through AssignPosition
    // above. Permissions is the denormalized cache rebuilt whenever either an
    // Account's Positions or its direct Roles change (resolved by the Application
    // layer across Position -> PositionRole -> Role and AccountRole -> Role, then
    // deduplicated) - kept here so JWT issuance never has to join across
    // Position/Role/PermissionDefinition at login time.
    // ============================================================================

    #region Role & Permission

    public void AssignRole(Role role)
    {
        if (AccountRoles.Any(ar => ar.RoleId == role.Id))
            throw ExceptionFactory.Duplicate("Account already has this role.");

        var accountRole = AccountRole.Create(Id, role.Id);
        AccountRoles.Add(accountRole);
    }

    public void RemoveRole(Guid roleId)
    {
        var accountRole = AccountRoles.FirstOrDefault(ar => ar.RoleId == roleId);
        if (accountRole is null)
            return;

        AccountRoles.Remove(accountRole);
    }

    /// <summary>
    /// Replaces the permission cache wholesale from the currently effective Positions' and
    /// directly-assigned Roles' merged, deduplicated permission set.
    /// </summary>
    public void RefreshPermissionSnapshot(
        IEnumerable<(PermissionKey Key, Guid SourceRoleId)> effectivePermissions)
    {
        Permissions.Clear();
        foreach (var (key, sourceRoleId) in effectivePermissions)
            Permissions.Add(AccountPermission.Create(Id, key, sourceRoleId));
    }

    #endregion

    // ============================================================================
    // MFA
    // Account-level MFA toggle. The actual MfaMethod rows (TOTP secret, backup
    // codes, ...) are owned/managed independently; this flag only gates whether
    // login flow requires a second factor at all.
    // ============================================================================

    #region MFA

    public void EnableMfa()
    {
        IsMfaEnabled = true;
    }

    public void DisableMfa()
    {
        IsMfaEnabled = false;
    }

    #endregion
}
