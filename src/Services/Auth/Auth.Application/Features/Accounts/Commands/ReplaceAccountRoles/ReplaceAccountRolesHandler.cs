using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Contract.Events.User;
using NovaCore.BuildingBlock.SharedKernel.Context;

namespace NovaCore.Auth.Application.Features.Accounts.Commands.ReplaceAccountRoles;

/// <summary>Two-phase commit, matching UpdateRolePermissionsHandler's own shape: the AccountRole
/// mutation commits first (AccountRoleAssignmentService self-commits), then this one Account's
/// recomputed effective permissions are enqueued as a second SaveChanges.</summary>
public sealed class ReplaceAccountRolesHandler(
    ICurrentUserService currentUserService,
    IAccountAuthorizationGuard authorizationGuard,
    IAccountReadService accountReadService,
    IAccountRoleAssignmentService accountRoleAssignmentService,
    IEffectivePermissionReadService effectivePermissionReadService,
    IOutboxStore outboxStore,
    IUnitOfWork unitOfWork) : ICommandHandler<ReplaceAccountRolesCommand>
{
    public async Task Handle(ReplaceAccountRolesCommand request, CancellationToken ct = default)
    {
        var tenantId = RequestContext.Current.TenantId ?? Guid.Empty;
        var actorId = currentUserService.GetUserId() ?? throw new UnauthorizedException();

        await authorizationGuard.EnsureCanManageAccountAsync(actorId, request.AccountId, tenantId, ct);

        // Only the newly-added roles need the "actor must already hold this role" check - a role
        // already assigned by someone with broader authority stays untouched by a caller who
        // doesn't personally hold it, as long as they aren't the one introducing it (see
        // IAccountAuthorizationGuard's doc comment).
        var currentRoleIds = await accountReadService.GetRoleIdsAsync(request.AccountId, ct);
        foreach (var roleId in request.RoleIds.Where(id => !currentRoleIds.Contains(id)))
            await authorizationGuard.EnsureCanGrantRoleAsync(actorId, roleId, tenantId, ct);

        var result = await accountRoleAssignmentService.ReplaceRolesAsync(request.AccountId, request.RoleIds, ct);
        if (!result.HasChanges)
            return;

        var effectivePermissions = await effectivePermissionReadService.GetEffectivePermissionsAsync(request.AccountId, tenantId, ct);

        await outboxStore.EnqueueAsync(
            new AccountEffectivePermissionsChangedIntegrationEvent(
                tenantId,
                [new AccountEffectivePermissions(request.AccountId, [.. effectivePermissions])]),
            ct);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
