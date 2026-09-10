using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Features.Accounts.DTOs;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Accounts.Write;

public sealed class AccountRoleAssignmentService(
    AuthDbContext dbContext,
    IUnitOfWork unitOfWork) : IAccountRoleAssignmentService, IPersistenceService
{
    public async Task<AccountRoleReplaceResult> ReplaceRolesAsync(
        Guid accountId,
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken ct = default)
    {
        var account = await dbContext.Users
            .Include(a => a.AccountRoles)
            .FirstOrDefaultAsync(a => a.Id == accountId, ct)
            ?? throw ExceptionFactory.EntityNotFound<Account>(accountId);

        var requestedIds = roleIds.ToHashSet();
        var requestedRoles = requestedIds.Count == 0
            ? []
            : await dbContext.Roles.Where(r => requestedIds.Contains(r.Id)).ToListAsync(ct);

        var currentRoleIds = account.AccountRoles.Select(ar => ar.RoleId).ToHashSet();
        var requestedRoleIds = requestedRoles.Select(r => r.Id).ToHashSet();

        var toRemove = currentRoleIds.Except(requestedRoleIds).ToArray();
        var toAdd = requestedRoles.Where(r => !currentRoleIds.Contains(r.Id)).ToArray();

        foreach (var roleId in toRemove)
            account.RemoveRole(roleId);

        foreach (var role in toAdd)
            account.AssignRole(role);

        var hasChanges = toRemove.Length > 0 || toAdd.Length > 0;
        if (hasChanges)
            await unitOfWork.SaveChangesAsync(ct);

        return new AccountRoleReplaceResult(hasChanges, requestedRoleIds);
    }
}
