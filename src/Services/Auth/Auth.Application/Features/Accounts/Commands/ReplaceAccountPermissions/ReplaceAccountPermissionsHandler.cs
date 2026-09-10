using NovaCore.Auth.Application.Abstractions.Authorization;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Context;

namespace NovaCore.Auth.Application.Features.Accounts.Commands.ReplaceAccountPermissions;

public sealed class ReplaceAccountPermissionsHandler(
    ICurrentUserService currentUserService,
    IAccountAuthorizationService accountAuthorizationService) : ICommandHandler<ReplaceAccountPermissionsCommand>
{
    public async Task Handle(ReplaceAccountPermissionsCommand request, CancellationToken ct = default)
    {
        var actorId = currentUserService.GetUserId() ?? throw new UnauthorizedException();
        var tenantId = RequestContext.Current.TenantId ?? Guid.Empty;

        await accountAuthorizationService.ReplacePermissionsAsync(actorId, request.AccountId, request.PermissionKeys, tenantId, ct);
    }
}
