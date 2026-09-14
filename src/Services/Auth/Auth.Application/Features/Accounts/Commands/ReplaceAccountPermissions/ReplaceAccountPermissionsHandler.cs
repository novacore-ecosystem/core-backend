using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Context;

namespace NovaCore.Auth.Application.Features.Accounts.Commands.ReplaceAccountPermissions;

public sealed class ReplaceAccountPermissionsHandler(
    ICurrentUserService currentUserService,
    IAccountAuthorizationService accountAuthorizationService,
    IUnitOfWork unitOfWork) : ICommandHandler<ReplaceAccountPermissionsCommand>
{
    public async Task Handle(ReplaceAccountPermissionsCommand request, CancellationToken ct = default)
    {
        var actorId = currentUserService.GetUserId()
            ?? throw new UnauthorizedException();
        var tenantId = RequestContext.Current.TenantId ?? Guid.Empty;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await accountAuthorizationService.ReplacePermissionsAsync(
                actorId,
                request.AccountId,
                request.PermissionKeys,
                tenantId,
                ct);
        }, ct: ct);
    }
}
