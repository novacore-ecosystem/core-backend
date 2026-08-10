namespace NovaCore.Auth.Domain.Entities.Accounts;

/// <summary>
/// Owned child of Account - a logged-in session spanning one or more RefreshToken rotations.
/// Retention-ready: ExpiresAt/Status are queryable by a future cleanup job without any
/// implementation here.
/// </summary>
public sealed class Session : BaseEntity<Guid>, ITenantEntity
{
    public Guid AccountId { get; private set; }
    public Account Account { get; private set; } = default!;
    public Guid? DeviceId { get; private set; }
    public Device? Device { get; private set; }
    public IpAddress IpAddress { get; private set; } = null!;
    public string? UserAgent { get; private set; }
    public SessionStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public RevocationReason? RevokedReason { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private Session() { }

    public static Session Create(
        Guid accountId,
        IpAddress ipAddress,
        DateTime expiresAt,
        Guid? deviceId = null,
        string? userAgent = null)
    {
        var now = DateTime.UtcNow;

        return new Session
        {
            Id = Guid.CreateVersion7(),
            AccountId = accountId,
            DeviceId = deviceId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Status = SessionStatus.Active,
            StartedAt = now,
            LastActivityAt = now,
            ExpiresAt = expiresAt,
        };
    }

    #region Lifecycle

    public void RefreshActivity()
    {
        if (Status != SessionStatus.Active)
            throw ExceptionFactory.InvalidStatus($"Cannot refresh activity on a {Status} session.");

        LastActivityAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        if (Status != SessionStatus.Active)
            return;

        Status = SessionStatus.Expired;
    }

    public void Revoke(RevocationReason reason)
    {
        if (Status is SessionStatus.Revoked or SessionStatus.LoggedOut)
            return;

        Status = SessionStatus.Revoked;
        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
    }

    public void LogOut()
    {
        if (Status is SessionStatus.Revoked or SessionStatus.LoggedOut)
            return;

        Status = SessionStatus.LoggedOut;
        RevokedAt = DateTime.UtcNow;
        RevokedReason = RevocationReason.UserLogout;
    }

    #endregion
}
