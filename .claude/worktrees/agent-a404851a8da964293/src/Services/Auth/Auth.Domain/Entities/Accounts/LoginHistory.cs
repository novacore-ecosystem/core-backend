namespace NovaCore.Auth.Domain.Entities.Accounts;

/// <summary>
/// Owned child of Account - an append-only record of a single login attempt. No mutation
/// methods; each attempt is recorded once and never changes afterward. Retention-ready via
/// AttemptedAt for a future cleanup job.
/// </summary>
public sealed class LoginHistory : BaseEntity<Guid>, ITenantEntity
{
    public Guid AccountId { get; private set; }
    public Account Account { get; private set; } = default!;
    public Guid? DeviceId { get; private set; }
    public IpAddress IpAddress { get; private set; } = null!;
    public string? UserAgent { get; private set; }
    public LoginResult Result { get; private set; }
    public DateTime AttemptedAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private LoginHistory() { }

    public static LoginHistory Record(
        Guid accountId,
        IpAddress ipAddress,
        LoginResult result,
        Guid? deviceId = null,
        string? userAgent = null)
    {
        return new LoginHistory
        {
            Id = Guid.CreateVersion7(),
            AccountId = accountId,
            DeviceId = deviceId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Result = result,
            AttemptedAt = DateTime.UtcNow,
        };
    }
}
