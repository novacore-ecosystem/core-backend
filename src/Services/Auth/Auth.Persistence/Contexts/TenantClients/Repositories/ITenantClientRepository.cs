using NovaCore.Auth.Domain.Entities.TenantClients;
using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.TenantClients.Repositories;

public interface ITenantClientRepository : IRepository<TenantClient>
{
    /// <summary>Every TenantClient for one Tenant, newest first - not expressible via the
    /// generic Get/GetMany overloads (ordering isn't supported generically), so it lives here
    /// as a custom method.</summary>
    Task<IReadOnlyList<TenantClient>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default);
}
