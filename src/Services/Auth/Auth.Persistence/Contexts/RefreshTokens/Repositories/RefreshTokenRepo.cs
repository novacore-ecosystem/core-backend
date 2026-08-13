using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Persistence.Ef.Repository;

namespace NovaCore.Auth.Persistence.Contexts.RefreshTokens.Repositories;

// Uses EntityGenericRepository (not AuthBaseRepository, unlike this folder's siblings) because
// RefreshTokenWriteService needs the by-id UpdateAsync(Guid id, ...) overload, which only exists
// on IRepository<TEntity, TId> - AuthBaseRepository<TEntity> only provides the predicate-based
// IRepository<TEntity> shape that the other repos in this project use.
public sealed class RefreshTokenRepo(AuthDbContext dbContext)
    : EntityGenericRepository<AuthDbContext, RefreshToken, Guid>(dbContext), IRefreshTokenRepository
{
}
