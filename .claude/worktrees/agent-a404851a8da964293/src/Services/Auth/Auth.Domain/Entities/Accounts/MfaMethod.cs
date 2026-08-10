using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Domain.Entities.Accounts;

/// <summary>
/// Owned child of Account - one enrolled second factor (TOTP app, SMS, email, or a backup-code
/// set). An Account may hold several; IsPrimary marks the one offered first at login.
/// </summary>
public sealed class MfaMethod : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid AccountId { get; private set; }
    public Account Account { get; private set; } = default!;
    public MfaMethodType Type { get; private set; }
    public string SecretEncrypted { get; private set; } = string.Empty;
    public bool IsVerified { get; private set; }
    public bool IsPrimary { get; private set; }
    public DateTime? LastUsedAt { get; private set; }

    public ICollection<MfaBackupCode> BackupCodes { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private MfaMethod() { }

    public static MfaMethod Create(
        Guid accountId,
        MfaMethodType type,
        string secretEncrypted)
    {
        ValidateSecret(secretEncrypted);

        return new MfaMethod
        {
            Id = Guid.CreateVersion7(),
            AccountId = accountId,
            Type = type,
            SecretEncrypted = secretEncrypted,
        };
    }

    #region Lifecycle

    public void Verify()
    {
        IsVerified = true;
    }

    public void MarkPrimary()
    {
        if (!IsVerified)
            throw ExceptionFactory.InvalidState("Cannot mark an unverified MFA method as primary.");

        IsPrimary = true;
    }

    public void RecordUsage()
    {
        LastUsedAt = DateTime.UtcNow;
    }

    #endregion

    public static bool IsValidSecret(string? secret) => secret.IsNotNullOrWhiteSpace();

    private static void ValidateSecret(string secret)
    {
        if (!IsValidSecret(secret))
            throw ExceptionFactory.RequiredField("MFA secret cannot be empty.");
    }
}
