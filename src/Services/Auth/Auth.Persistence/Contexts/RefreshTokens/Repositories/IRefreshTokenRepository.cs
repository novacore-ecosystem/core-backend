using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.RefreshTokens.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken, Guid>
{
    /// <summary>Every non-revoked RefreshToken for one Account, newest first - not expressible
    /// via the generic Get/GetMany overloads (filter + ordering aren't supported generically),
    /// so it lives here as a custom method.</summary>
    Task<List<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
