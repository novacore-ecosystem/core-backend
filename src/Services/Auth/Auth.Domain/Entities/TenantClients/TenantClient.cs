using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Domain.Entities.TenantClients;

/// <summary>
/// Independent aggregate root - a public client identity for one Tenant (Web/Mobile/Admin client,
/// ...), letting an unauthenticated caller resolve which Tenant it belongs to before login:
/// PublicKey -> TenantClient -> TenantId -> username/password -> access token. Mirrors Scope's
/// shape (own aggregate, plain TenantId FK, no back-collection on Tenant) rather than being an
/// owned child of Tenant - client credentials are independently created/rotated/revoked, not
/// bootstrap data that lives and dies with the tenant record itself.
///
/// Deliberately does NOT implement ITenantEntity. The Entity Convention's query filter compares
/// TenantId against RequestContext.Current.TenantId, which is Guid.Empty for exactly the
/// anonymous, pre-authentication requests this aggregate exists to serve (see
/// docs/reference/tenant-convention.md) - opting in would make every PublicKey lookup return zero
/// rows, since no real TenantClient row ever has TenantId == Guid.Empty. TenantId is still a plain
/// FK, assigned once in Create and never touched by TenantAssignmentInterceptor.
///
/// PublicKey is not a secret or an authorization credential - it only answers "which Tenant is
/// this client associated with," never "is this client allowed to do X." Business authorization
/// stays entirely on the access token issued after username/password authentication.
/// </summary>
public sealed class TenantClient : AggregateRoot<Guid>, IAuditable
{
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public ClientPublicKey PublicKey { get; private set; } = null!;
    public TenantClientStatus Status { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public RevocationReason? RevokedReason { get; private set; }

    private TenantClient() { }

    public static TenantClient Create(
        Guid tenantId,
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

    /// <summary>Domain-level truth for "can this client still be used to resolve a Tenant" - not
    /// enforcement (no Redis/Gateway/cache check here, see docs/services/auth-service.md), just the
    /// invariant a later phase's resolution query relies on. Treats a past ExpiresAt as unusable
    /// even before a cleanup job has flipped Status to Expired, same as Invitation.Accept() checks
    /// ExpiresAt directly rather than trusting only the stored Status.</summary>
    public bool IsUsable() =>
        Status == TenantClientStatus.Active && (ExpiresAt is null || ExpiresAt > DateTime.UtcNow);

    /// <summary>Idempotent no-op once already Revoked/Expired - key rotation and an admin-forced
    /// revoke can race harmlessly, same shape as Session.Revoke.</summary>
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

    public static bool IsValidName(string? name) => name.IsNotNullOrWhiteSpace();

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Client name cannot be empty.");
    }

    private static void ValidateTenantId(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw ExceptionFactory.RequiredField("TenantId cannot be empty.");
    }
}
