using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Context;

namespace NovaCore.Auth.Application.Features.Accounts.Commands.SetAccountLevel;

/// <summary>Self-assignment is rejected unconditionally, even for an actor holding Permissions.Root
/// - the one rule IAccountAuthorizationGuard's Root bypass deliberately does not cover, per the
/// explicit "an account must never be able to change its own Level" requirement.</summary>
public sealed class SetAccountLevelHandler(
    ICurrentUserService currentUserService,
    IAccountAuthorizationGuard authorizationGuard,
    IAccountReadService accountReadService,
    IAccountWriteService accountWriteService) : ICommandHandler<SetAccountLevelCommand>
{
    public async Task Handle(SetAccountLevelCommand request, CancellationToken ct = default)
    {
        var tenantId = RequestContext.Current.TenantId ?? Guid.Empty;
        var actorId = currentUserService.GetUserId() ?? throw new UnauthorizedException();

        if (actorId == request.AccountId)
            throw new ForbiddenException("You cannot change your own account level.");

        await authorizationGuard.EnsureCanManageAccountAsync(actorId, request.AccountId, tenantId, ct);

        var actor = await accountReadService.GetByIdAsync(actorId, ct)
            ?? throw new NotFoundException("Account", actorId);
        if (actor.Level <= request.Level)
            throw new ForbiddenException("You cannot grant a level you do not outrank.");

        await accountWriteService.SetLevelAsync(request.AccountId, request.Level, ct);
    }
}
