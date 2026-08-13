using NovaCore.Auth.Domain.Entities.TenantClients;

namespace NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;

public interface ITenantClientReadService
{
    Task<TenantClient?> GetByPublicKeyAsync(string publicKey, CancellationToken ct = default);

    Task<bool> ExistsByPublicKeyAsync(string publicKey, CancellationToken ct = default);

    /// <summary>Admin-facing "this tenant's clients" view (Tenant Detail, rotation) - PublicKey is
    /// safe to include (see TenantClient's class doc comment), there is no secret to redact.</summary>
    Task<IReadOnlyList<TenantClient>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default);
}
