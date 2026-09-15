using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.LoginHistories.Repositories;

public interface ILoginHistoryRepository : IRepository<LoginHistory>
{
}
