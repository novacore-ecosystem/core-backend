using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;
using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.Permissions.Read;

public sealed class PermissionReadService(AuthDbContext dbContext) : IPermissionReadService
{
    public async Task<PermissionDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.PermissionDefinitions
            .AsNoTracking()
            .Include(p => p.PermissionGroup)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<PermissionDefinition>> ListAsync(CancellationToken ct = default)
    {
        return await dbContext.PermissionDefinitions
            .AsNoTracking()
            .Include(p => p.PermissionGroup)
            .OrderBy(p => p.PermissionGroup.SortOrder)
            .ThenBy(p => p.Key.Value)
            .ToListAsync(ct);
    }
}
