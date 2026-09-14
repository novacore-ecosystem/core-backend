using Microsoft.EntityFrameworkCore;
using NovaCore.Auth.Domain.Entities.TenantClients;
using NovaCore.Auth.Persistence.Contexts;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.TenantClients.Repositories;

public sealed class TenantClientRepository(AuthDbContext dbContext)
    : AuthBaseRepository<TenantClient>(dbContext), ITenantClientRepository
{
    public async Task<IReadOnlyList<TenantClient>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }
}
