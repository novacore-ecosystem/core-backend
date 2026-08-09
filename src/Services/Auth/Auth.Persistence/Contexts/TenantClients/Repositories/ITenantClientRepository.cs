using NovaCore.Auth.Domain.Entities.TenantClients;

using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.TenantClients.Repositories;

public interface ITenantClientRepository : IRepository<TenantClient>
{
    // Leave empty for now - only generic CRUD is needed. Reserved for future scaling
    // (bulk workflows keyed by something other than the primary key).
}
