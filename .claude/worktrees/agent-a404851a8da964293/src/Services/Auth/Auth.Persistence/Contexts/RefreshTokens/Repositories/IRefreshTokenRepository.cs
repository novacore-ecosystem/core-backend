using NovaCore.Auth.Domain.Entities.Accounts;

using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.RefreshTokens.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
