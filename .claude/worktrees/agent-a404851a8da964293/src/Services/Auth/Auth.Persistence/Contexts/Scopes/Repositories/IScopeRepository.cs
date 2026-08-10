using NovaCore.Auth.Domain.Entities.Scopes;

using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.Scopes.Repositories;

public interface IScopeRepository : IRepository<Scope>
{
    // Leave empty for now - only generic CRUD is needed. Reserved for future scaling
    // (bulk workflows keyed by something other than the primary key).
}
