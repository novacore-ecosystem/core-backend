namespace NovaCore.Auth.Application.Features.Tenants.Commands.RotateTenantClient;

/// <summary>Revokes every currently-Active TenantClient for the tenant (RevocationReason.Superseded)
/// and issues a new one - see RotateTenantClientHandler. Name defaults to the revoked client's
/// name when exactly one was active.</summary>
public sealed record RotateTenantClientCommand(Guid TenantId, string? Name = null) : ICommand<TenantClientRotationResponse>;

/// <summary>The new PublicKey - safe to return, it is not a secret (see TenantClient's class doc
/// comment). The previous key is never returned.</summary>
public sealed record TenantClientRotationResponse(
    Guid ClientId,
    string PublicKey,
    DateTime? ExpiresAt);
