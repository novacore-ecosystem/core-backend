using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Persistence.Contexts.Accounts.Repositories;
using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Accounts.Read;

public sealed class AccountReadService(IAccountRepository accountRepo) : IAccountReadService, IPersistenceService
{
    public async Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await accountRepo.GetAsync(
            u => u.Email == email,
            q => q.Include(u => u.AccountRoles),
            ct);
    }

    public async Task<Account?> GetByEmailAsync(string email, Guid tenantId, CancellationToken ct = default)
    {
        return await accountRepo.GetAsync(
            u => u.Email == email && u.TenantId == tenantId,
            q => q.Include(u => u.AccountRoles),
            ct);
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await accountRepo.GetAsync(u => u.Id == id, ct);
    }

    public async Task<IReadOnlySet<Guid>> GetRoleIdsAsync(Guid accountId, CancellationToken ct = default)
    {
        return await accountRepo.GetRoleIdsAsync(accountId, ct);
    }

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid accountId, CancellationToken ct = default)
    {
        return await accountRepo.GetRoleNamesAsync(accountId, ct);
    }
}
