using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.Roles.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    /// <summary>Every Role, ordered by name - not expressible via the generic Get/GetMany
    /// overloads (ordering isn't supported generically), so it lives here as a custom method.</summary>
    Task<IReadOnlyList<Role>> ListAsync(CancellationToken ct = default);

    /// <summary>Every AccountId directly assigned this Role - queries the AccountRole join, not
    /// the Role entity itself, so it isn't expressible generically.</summary>
    Task<IReadOnlyCollection<Guid>> GetAccountIdsAsync(Guid roleId, CancellationToken ct = default);
}
