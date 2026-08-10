using NovaCore.Auth.Domain.Entities.Tenants;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

public interface ITenantReadService
{
    Task<Tenant?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
}
