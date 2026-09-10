using NovaCore.Auth.Application.Abstractions.Authorization;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Context;

namespace NovaCore.Auth.Application.Features.Accounts.Commands.ReplaceAccountRoles;

public sealed class ReplaceAccountRolesHandler(
    ICurrentUserService currentUserService,
    IAccountAuthorizationService accountAuthorizationService) : ICommandHandler<ReplaceAccountRolesCommand>
{
    public async Task Handle(ReplaceAccountRolesCommand request, CancellationToken ct = default)
    {
        var actorId = currentUserService.GetUserId() ?? throw new UnauthorizedException();
        var tenantId = RequestContext.Current.TenantId ?? Guid.Empty;

        await accountAuthorizationService.ReplaceRolesAsync(actorId, request.AccountId, request.RoleIds, tenantId, ct);
    }
}
