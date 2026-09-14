using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.Accounts.Repositories;

public interface IAccountRepository : IRepository<Account>
{
    Task DeleteIfExistAsync(Guid id, CancellationToken ct = default);

    /// <summary>Every RoleId assigned to one Account - not expressible via the generic Get/GetMany
    /// overloads (this queries the AccountRole join, not the Account entity itself), so it lives
    /// here as a custom method.</summary>
    Task<IReadOnlySet<Guid>> GetRoleIdsAsync(Guid accountId, CancellationToken ct = default);

    /// <summary>Every Role name assigned to one Account (joined from AccountRole to Role) - same
    /// reasoning as GetRoleIdsAsync.</summary>
    Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid accountId, CancellationToken ct = default);

    /// <summary>Whether the Account is a member of the App - queries the AccountApp join, not
    /// the Account entity itself, so it isn't expressible generically.</summary>
    Task<bool> IsAssignedToAppAsync(Guid accountId, Guid appId, CancellationToken ct = default);
}
