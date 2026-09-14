using Microsoft.EntityFrameworkCore;
using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.Auth.Persistence.Contexts;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.Roles.Repositories;

public sealed class RoleRepository(AuthDbContext dbContext)
    : AuthBaseRepository<Role>(dbContext), IRoleRepository
{
    public async Task<IReadOnlyList<Role>> ListAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Guid>> GetAccountIdsAsync(Guid roleId, CancellationToken ct = default)
    {
        return await _dbContext.UserRoles
            .AsNoTracking()
            .Where(ar => ar.RoleId == roleId)
            .Select(ar => ar.UserId)
            .ToListAsync(ct);
    }
}
