using Microsoft.EntityFrameworkCore;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Persistence.Engine;
using NovaCore.BuildingBlock.Persistence.Ef.Repository;

namespace NovaCore.Auth.Persistence.Contexts.RefreshTokens.Repositories;

// Uses EntityGenericRepository (not AuthBaseRepository, unlike this folder's siblings) because
// RefreshTokenWriteService needs the by-id UpdateAsync(Guid id, ...) overload, which only exists
// on IRepository<TEntity, TId> - AuthBaseRepository<TEntity> only provides the predicate-based
// IRepository<TEntity> shape that the other repos in this project use.
public sealed class RefreshTokenRepository(AuthDbContext dbContext)
    : EntityGenericRepository<AuthDbContext, RefreshToken, Guid>(dbContext), IRefreshTokenRepository
{
    public async Task<List<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(rt => rt.AccountId == userId && !rt.IsRevoked)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(ct);
    }
}
