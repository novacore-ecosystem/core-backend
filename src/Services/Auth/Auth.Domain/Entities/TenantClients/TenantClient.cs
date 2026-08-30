using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Domain.Entities.TenantClients;

public sealed class TenantClient : AggregateRoot<Guid>, IAuditable
{
    public Guid? TenantId { get; private set; }
    public Tenant? Tenant { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ClientPublicKey PublicKey { get; private set; } = null!;
    public TenantClientStatus Status { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public RevocationReason? RevokedReason { get; private set; }

    public bool IsRootClient => TenantId is null;

    private TenantClient() { }

    /// <summary>
    /// tenantId null creates the Root client; a non-empty value creates a tenant client.
    /// Guid.Empty is rejected outright - it is not a valid "no tenant" sentinel here, null is.
    /// </summary>
    public static TenantClient Create(
        Guid? tenantId,
        string name,
        DateTime? expiresAt = null)
    {
        ValidateTenantId(tenantId);
        ValidateName(name);

        return new TenantClient
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Name = name,
            PublicKey = ClientPublicKey.Generate(),
            Status = TenantClientStatus.Active,
            ExpiresAt = expiresAt,
        };
    }

    #region Lifecycle

    /// <summary>
    /// Domain-level truth for "can this client still be used to resolve a Tenant" - not
    /// enforcement (no Redis/Gateway/cache check here, see docs/services/auth-service.md), just the
    /// invariant a later phase's resolution query relies on. Treats a past ExpiresAt as unusable
    /// even before a cleanup job has flipped Status to Expired, same as Invitation.Accept() checks
    /// ExpiresAt directly rather than trusting only the stored Status.
    /// </summary>
    public bool IsUsable() =>
        Status == TenantClientStatus.Active && (ExpiresAt is null || ExpiresAt > DateTime.UtcNow);

    /// <summary>
    /// Idempotent no-op once already Revoked/Expired - key rotation and an admin-forced
    /// revoke can race harmlessly, same shape as Session.Revoke.
    /// </summary>
    public void Revoke(RevocationReason reason)
    {
        if (Status != TenantClientStatus.Active)
            return;

        Status = TenantClientStatus.Revoked;
        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
    }

    public void MarkExpired()
    {
        if (Status != TenantClientStatus.Active)
            return;

        Status = TenantClientStatus.Expired;
    }

    #endregion

    public static bool IsValidName(string? name)
        => name.IsNotNullOrWhiteSpace();

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Client name cannot be empty.");
    }

    private static void ValidateTenantId(Guid? tenantId)
    {
        if (tenantId == Guid.Empty)
            throw ExceptionFactory.RequiredField("TenantId cannot be Guid.Empty - pass null for the Root client instead.");
    }
}
