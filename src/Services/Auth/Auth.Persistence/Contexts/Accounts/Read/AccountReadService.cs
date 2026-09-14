using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Persistence.Engine;

using Microsoft.EntityFrameworkCore;

using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Accounts.Read;

public sealed class AccountReadService(AuthDbContext dbContext) : IAccountReadService, IPersistenceService
{
    public async Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Include(u => u.AccountRoles)
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<Account?> GetByEmailAsync(string email, Guid tenantId, CancellationToken ct = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Include(u => u.AccountRoles)
            .FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenantId, ct);
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<IReadOnlySet<Guid>> GetRoleIdsAsync(Guid accountId, CancellationToken ct = default)
    {
        var roleIds = await dbContext.UserRoles
            .Where(ar => ar.UserId == accountId)
            .Select(ar => ar.RoleId)
            .ToListAsync(ct);

        return roleIds.ToHashSet();
    }

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid accountId, CancellationToken ct = default)
    {
        return await dbContext.UserRoles
            .Where(ar => ar.UserId == accountId)
            .Join(dbContext.Roles, ar => ar.RoleId, r => r.Id, (ar, r) => r.Name!)
            .ToListAsync(ct);
    }
}
