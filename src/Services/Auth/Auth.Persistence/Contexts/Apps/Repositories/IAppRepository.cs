using NovaCore.Auth.Domain.Entities.Apps;

using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.Apps.Repositories;

public interface IAppRepository : IRepository<App>
{
    // Leave empty for now - only generic CRUD is needed. Reserved for future scaling
    // (bulk workflows keyed by something other than the primary key).
}
