using Microsoft.EntityFrameworkCore;
using NovaCore.Auth.Domain.Entities.Registrations;
using NovaCore.Auth.Persistence.Contexts;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.Registrations.Repositories;

public sealed class RegistrationDefaultPermissionRepository(AuthDbContext dbContext)
    : AuthBaseRepository<RegistrationDefaultPermission>(dbContext), IRegistrationDefaultPermissionRepository
{
    public async Task<IReadOnlyCollection<string>> GetPermissionKeysAsync(Guid tenantId, Guid appId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.AppId == appId)
            .Select(x => x.PermissionDefinition.Key.Value)
            .ToListAsync(ct);
    }

    public async Task<List<RegistrationDefaultPermission>> ListForAppAsync(Guid tenantId, Guid appId, CancellationToken ct = default)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .Include(x => x.PermissionDefinition)
            .Where(x => x.TenantId == tenantId && x.AppId == appId)
            .ToListAsync(ct);
    }

    public Task RemoveRangeAsync(IEnumerable<RegistrationDefaultPermission> entities, CancellationToken ct = default)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }
}
