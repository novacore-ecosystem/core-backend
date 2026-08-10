using NovaCore.Auth.Domain.Entities.Tenants;

using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.Tenants.Repositories;

public interface ITenantRepository : IRepository<Tenant>
{
    // Leave empty for now - only generic CRUD is needed. Reserved for future scaling
    // (bulk workflows keyed by something other than the primary key).
}
