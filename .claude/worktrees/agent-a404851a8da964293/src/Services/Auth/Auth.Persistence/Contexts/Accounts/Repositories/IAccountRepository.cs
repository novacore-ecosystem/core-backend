using NovaCore.Auth.Domain.Entities.Accounts;

using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.Accounts.Repositories;

public interface IAccountRepository : IRepository<Account>
{
    Task DeleteIfExistAsync(Guid id, CancellationToken ct = default);
}
