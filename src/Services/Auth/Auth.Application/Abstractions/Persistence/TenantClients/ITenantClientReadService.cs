using NovaCore.Auth.Domain.Entities.TenantClients;

namespace NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;

public interface ITenantClientReadService
{
    Task<TenantClient?> GetByPublicKeyAsync(string publicKey, CancellationToken ct = default);

    Task<bool> ExistsByPublicKeyAsync(string publicKey, CancellationToken ct = default);
}
