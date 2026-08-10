using NovaCore.Auth.Application.Abstractions.Persistence.RefreshTokens;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Persistence.Engine;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.Auth.Persistence.Contexts.RefreshTokens.Read;

public sealed class RefreshTokenReadService(AuthDbContext dbContext) : IRefreshTokenReadService
{
    public async Task<List<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await dbContext.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.AccountId == userId && !rt.IsRevoked)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(ct);
    }
}
