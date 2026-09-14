using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Context;

namespace NovaCore.Auth.Application.Features.Accounts.Commands.ReplaceAccountRoles;

public sealed class ReplaceAccountRolesHandler(
    ICurrentUserService currentUserService,
    IAccountAuthorizationService accountAuthorizationService,
    IUnitOfWork unitOfWork) : ICommandHandler<ReplaceAccountRolesCommand>
{
    public async Task Handle(ReplaceAccountRolesCommand request, CancellationToken ct = default)
    {
        var actorId = currentUserService.GetUserId()
            ?? throw new UnauthorizedException();
        var tenantId = RequestContext.Current.TenantId ?? Guid.Empty;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await accountAuthorizationService.ReplaceRolesAsync(
                actorId,
                request.AccountId,
                request.RoleIds,
                tenantId,
                ct);
        }, ct: ct);
    }
}
