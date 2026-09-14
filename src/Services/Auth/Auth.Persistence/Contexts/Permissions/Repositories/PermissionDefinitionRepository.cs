using Microsoft.EntityFrameworkCore;
using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Persistence.Contexts;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.Permissions.Repositories;

public sealed class PermissionDefinitionRepository(AuthDbContext dbContext)
    : AuthBaseRepository<PermissionDefinition>(dbContext), IPermissionDefinitionRepository
{
    public async Task<IReadOnlyList<PermissionDefinition>> ListAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(p => p.PermissionGroup)
            .OrderBy(p => p.PermissionGroup.SortOrder)
            .ThenBy(p => p.Key.Value)
            .ToListAsync(ct);
    }
}
