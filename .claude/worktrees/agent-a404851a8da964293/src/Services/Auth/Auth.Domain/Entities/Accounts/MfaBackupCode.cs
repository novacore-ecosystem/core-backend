namespace NovaCore.Auth.Domain.Entities.Accounts;

/// <summary>
/// Owned child of MfaMethod - a single one-time backup code.
/// Consumed exactly once.
/// </summary>
public sealed class MfaBackupCode : BaseEntity<Guid>, ITenantEntity
{
    public Guid MfaMethodId { get; private set; }
    public MfaMethod MfaMethod { get; private set; } = default!;
    public string CodeHash { get; private set; } = string.Empty;
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private MfaBackupCode() { }

    public static MfaBackupCode Create(Guid mfaMethodId, string codeHash)
    {
        return new MfaBackupCode
        {
            Id = Guid.CreateVersion7(),
            MfaMethodId = mfaMethodId,
            CodeHash = codeHash,
        };
    }

    public void Consume()
    {
        if (IsUsed)
            throw ExceptionFactory.InvalidState("Backup code has already been used.");

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }
}
