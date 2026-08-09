using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Persistence.Roles;
using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.Roles.Read;

public sealed class RoleReadService(AuthDbContext dbContext) : IRoleReadService
{
    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Roles
            .AsNoTracking()
            .Include(r => r.Permissions)
                .ThenInclude(p => p.PermissionDefinition)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<IReadOnlyList<Role>> ListAsync(CancellationToken ct = default)
    {
        return await dbContext.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
    }
}
