using Microsoft.EntityFrameworkCore;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.Accounts.Repositories;

public sealed class AccountRepository(AuthDbContext dbContext)
    : AuthBaseRepository<Account>(dbContext), IAccountRepository
{
    public async Task DeleteIfExistAsync(Guid id, CancellationToken ct = default)
    {
        await _dbContext.Users
            .Where(u => u.Id == id)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<IReadOnlySet<Guid>> GetRoleIdsAsync(Guid accountId, CancellationToken ct = default)
    {
        var roleIds = await _dbContext.UserRoles
            .Where(ar => ar.UserId == accountId)
            .Select(ar => ar.RoleId)
            .ToListAsync(ct);

        return roleIds.ToHashSet();
    }

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid accountId, CancellationToken ct = default)
    {
        return await _dbContext.UserRoles
            .Where(ar => ar.UserId == accountId)
            .Join(_dbContext.Roles, ar => ar.RoleId, r => r.Id, (ar, r) => r.Name!)
            .ToListAsync(ct);
    }

    public async Task<bool> IsAssignedToAppAsync(Guid accountId, Guid appId, CancellationToken ct = default)
    {
        return await _dbContext.AccountApps
            .AsNoTracking()
            .AnyAsync(aa => aa.AccountId == accountId && aa.AppId == appId, ct);
    }
}
