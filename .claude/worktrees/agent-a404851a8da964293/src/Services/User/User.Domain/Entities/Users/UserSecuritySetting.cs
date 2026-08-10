namespace NovaCore.User.Domain.Entities.Users;

/// <summary>Owned 1:1 extension of User holding account-security policy and recovery contacts.</summary>
public sealed class UserSecuritySetting : BaseEntity, IAuditable, ITenantEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public bool TwoFactorEnabled { get; private set; }
    public bool RequirePasswordRotation { get; private set; }
    public bool AllowRememberDevice { get; private set; } = true;
    public bool TrustedDevicesOnly { get; private set; }
    public string? RecoveryEmail { get; private set; }
    public string? RecoveryPhone { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserSecuritySetting() { }

    public static UserSecuritySetting Create(
        Guid userId,
        bool twoFactorEnabled = false,
        bool requirePasswordRotation = false,
        bool allowRememberDevice = true,
        bool trustedDevicesOnly = false,
        string? recoveryEmail = null,
        string? recoveryPhone = null)
    {
        return new UserSecuritySetting
        {
            UserId = userId,
            TwoFactorEnabled = twoFactorEnabled,
            RequirePasswordRotation = requirePasswordRotation,
            AllowRememberDevice = allowRememberDevice,
            TrustedDevicesOnly = trustedDevicesOnly,
            RecoveryEmail = recoveryEmail,
            RecoveryPhone = recoveryPhone,
        };
    }

    // ============================================================================
    // Two-factor authentication
    // Dedicated toggle methods for the account's most consequential security
    // flag, kept separate from the bulk UpdateDetails upsert.
    // ============================================================================

    #region Two-factor authentication

    public void EnableTwoFactor()
    {
        TwoFactorEnabled = true;
    }

    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Password-rotation policy, device-trust policy, and recovery contact
    // details.
    // ============================================================================

    #region Details & lifecycle

    public void UpdateDetails(
        bool requirePasswordRotation,
        bool allowRememberDevice,
        bool trustedDevicesOnly,
        string? recoveryEmail,
        string? recoveryPhone)
    {
        RequirePasswordRotation = requirePasswordRotation;
        AllowRememberDevice = allowRememberDevice;
        TrustedDevicesOnly = trustedDevicesOnly;
        RecoveryEmail = recoveryEmail;
        RecoveryPhone = recoveryPhone;
    }

    #endregion
}
