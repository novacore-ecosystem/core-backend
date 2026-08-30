namespace NovaCore.Auth.Domain.Entities.TokenBlacklists;

public sealed class TokenBlacklist : BaseEntity<Guid>, ITenantEntity
{
    public Guid Jti { get; private set; }
    public Guid AccountId { get; private set; }
    public RevocationReason Reason { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime BlacklistedAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private TokenBlacklist() { }

    public static TokenBlacklist Create(
        Guid jti,
        Guid accountId,
        RevocationReason reason,
        DateTime expiresAt)
    {
        return new TokenBlacklist
        {
            Id = Guid.CreateVersion7(),
            Jti = jti,
            AccountId = accountId,
            Reason = reason,
            ExpiresAt = expiresAt,
            BlacklistedAt = DateTime.UtcNow,
        };
    }
}
