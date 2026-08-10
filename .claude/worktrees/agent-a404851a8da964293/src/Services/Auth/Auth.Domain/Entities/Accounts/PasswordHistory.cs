namespace NovaCore.Auth.Domain.Entities.Accounts;

/// <summary>
/// Owned child of Account - an append-only record of a previously used password hash,
/// consulted to enforce password-reuse policy. No mutation methods.
/// </summary>
public sealed class PasswordHistory : BaseEntity<Guid>, ITenantEntity
{
    public Guid AccountId { get; private set; }
    public Account Account { get; private set; } = default!;
    public string PasswordHash { get; private set; } = string.Empty;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private PasswordHistory() { }

    internal static PasswordHistory Record(Guid accountId, string passwordHash)
    {
        return new PasswordHistory
        {
            Id = Guid.CreateVersion7(),
            AccountId = accountId,
            PasswordHash = passwordHash,
        };
    }
}
