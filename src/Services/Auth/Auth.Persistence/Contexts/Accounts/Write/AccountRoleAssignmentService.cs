using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Features.Accounts.DTOs;
using NovaCore.Auth.Persistence.Contexts.Accounts.Repositories;
using NovaCore.Auth.Persistence.Contexts.Roles.Repositories;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Accounts.Write;

public sealed class AccountRoleAssignmentService(
    IAccountRepository accountRepo,
    IRoleRepository roleRepo,
    IUnitOfWork unitOfWork) : IAccountRoleAssignmentService, IPersistenceService
{
    public async Task<AccountRoleReplaceResult> ReplaceRolesAsync(
        Guid accountId,
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken ct = default)
    {
        var requestedIds = roleIds.ToHashSet();
        var requestedRoles = requestedIds.Count == 0
            ? []
            : await roleRepo.GetManyAsync(r => r.Id, requestedIds, ct);
        var requestedRoleIds = requestedRoles.Select(r => r.Id).ToHashSet();

        var hasChanges = false;
        await accountRepo.UpdateAsync(
            a => a.Id == accountId,
            q => q.Include(a => a.AccountRoles),
            account =>
            {
                var currentRoleIds = account.AccountRoles.Select(ar => ar.RoleId).ToHashSet();

                var toRemove = currentRoleIds.Except(requestedRoleIds).ToArray();
                var toAdd = requestedRoles.Where(r => !currentRoleIds.Contains(r.Id)).ToArray();

                foreach (var roleId in toRemove)
                    account.RemoveRole(roleId);

                foreach (var role in toAdd)
                    account.AssignRole(role);

                hasChanges = toRemove.Length > 0 || toAdd.Length > 0;
            },
            ct);

        if (hasChanges)
            await unitOfWork.SaveChangesAsync(ct);

        return new AccountRoleReplaceResult(hasChanges, requestedRoleIds);
    }

    public async Task RemoveRoleAsync(Guid accountId, Guid roleId, CancellationToken ct = default)
    {
        var hasChanges = false;
        await accountRepo.UpdateAsync(
            a => a.Id == accountId,
            q => q.Include(a => a.AccountRoles),
            account =>
            {
                if (!account.AccountRoles.Any(ar => ar.RoleId == roleId))
                    return;

                account.RemoveRole(roleId);
                hasChanges = true;
            },
            ct);

        if (hasChanges)
            await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyCollection<Guid>> GetAccountIdsInRoleAsync(Guid roleId, CancellationToken ct = default)
    {
        return await roleRepo.GetAccountIdsAsync(roleId, ct);
    }
}
