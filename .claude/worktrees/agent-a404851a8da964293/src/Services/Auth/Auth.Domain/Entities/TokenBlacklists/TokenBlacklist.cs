namespace NovaCore.Auth.Domain.Entities.TokenBlacklists;

/// <summary>
/// Access-token jti denylist entry, for revoking a short-lived access token before its natural
/// expiry (e.g. forced logout, compromised token) - distinct from RefreshToken.IsRevoked, which only
/// stops future refreshes. ExpiresAt mirrors the token's original expiry; retention-ready since an
/// entry is irrelevant once the token would have expired anyway.
/// </summary>
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
