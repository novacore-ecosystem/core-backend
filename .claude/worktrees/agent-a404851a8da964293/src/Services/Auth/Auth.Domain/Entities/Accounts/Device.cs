using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Domain.Entities.Accounts;

/// <summary>
/// Owned child of Account - a recognized physical/browser device. Sessions optionally link
/// back to the Device they were opened from, and trust status feeds MFA-skip / suspicious-login
/// decisions at the Application layer.
/// </summary>
public sealed class Device : BaseEntity<Guid>, ITenantEntity
{
    public Guid AccountId { get; private set; }
    public Account Account { get; private set; } = default!;
    public string Fingerprint { get; private set; } = string.Empty;
    public string DeviceName { get; private set; } = SessionDefaults.UnknownDeviceName;
    public DeviceType DeviceType { get; private set; }
    public string? Platform { get; private set; }
    public bool IsTrusted { get; private set; }
    public DateTime LastSeenAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private Device() { }

    public static Device Create(
        Guid accountId,
        string fingerprint,
        string deviceName,
        DeviceType deviceType,
        string? platform = null)
    {
        ValidateFingerprint(fingerprint);

        return new Device
        {
            Id = Guid.CreateVersion7(),
            AccountId = accountId,
            Fingerprint = fingerprint,
            DeviceName = deviceName.IsNotNullOrWhiteSpace()
                ? deviceName
                : SessionDefaults.UnknownDeviceName,
            DeviceType = deviceType,
            Platform = platform,
            LastSeenAt = DateTime.UtcNow,
        };
    }

    #region Lifecycle

    public void Trust()
    {
        IsTrusted = true;
    }

    public void Untrust()
    {
        IsTrusted = false;
    }

    public void RecordActivity()
    {
        LastSeenAt = DateTime.UtcNow;
    }

    #endregion

    public static bool IsValidFingerprint(string? fingerprint) => fingerprint.IsNotNullOrWhiteSpace();

    private static void ValidateFingerprint(string fingerprint)
    {
        if (!IsValidFingerprint(fingerprint))
            throw ExceptionFactory.RequiredField("Device fingerprint cannot be empty.");
    }
}
