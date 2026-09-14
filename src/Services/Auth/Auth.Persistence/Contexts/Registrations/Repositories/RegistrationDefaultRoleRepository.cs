using Microsoft.EntityFrameworkCore;
using NovaCore.Auth.Domain.Entities.Registrations;
using NovaCore.Auth.Persistence.Contexts;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.Registrations.Repositories;

public sealed class RegistrationDefaultRoleRepository(AuthDbContext dbContext)
    : AuthBaseRepository<RegistrationDefaultRole>(dbContext), IRegistrationDefaultRoleRepository
{
    public async Task<IReadOnlyCollection<Guid>> GetRoleIdsAsync(Guid tenantId, Guid appId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.AppId == appId)
            .Select(x => x.RoleId)
            .ToListAsync(ct);
    }

    public async Task<List<RegistrationDefaultRole>> ListForAppAsync(Guid tenantId, Guid appId, CancellationToken ct = default)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.AppId == appId)
            .ToListAsync(ct);
    }

    public Task RemoveRangeAsync(IEnumerable<RegistrationDefaultRole> entities, CancellationToken ct = default)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }
}
