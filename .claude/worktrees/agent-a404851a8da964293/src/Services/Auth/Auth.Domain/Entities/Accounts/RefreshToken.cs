namespace NovaCore.Auth.Domain.Entities.Accounts;

/// <summary>
/// Owned child of Account - a refresh token used to obtain new access tokens.
/// </summary>
public sealed class RefreshToken : BaseEntity<Guid>, ITenantEntity
{
    public Guid AccountId { get; private set; }
    public Account Account { get; private set; } = null!;
    public Guid? SessionId { get; private set; }
    public Session? Session { get; private set; }
    public Guid JwtId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiryDate { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public RevocationReason? RevokedReason { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private RefreshToken() { }

    public static RefreshToken Create(
        Guid jwtId,
        string token,
        DateTime expiryDate,
        Guid userId,
        Guid? sessionId = null)
    {
        return new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            JwtId = jwtId,
            Token = token,
            ExpiryDate = expiryDate,
            AccountId = userId,
            SessionId = sessionId,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void Revoke(RevocationReason reason)
    {
        if (IsRevoked)
            return;

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
    }
}
