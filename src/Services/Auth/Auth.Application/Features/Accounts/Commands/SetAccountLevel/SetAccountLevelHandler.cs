using NovaCore.Auth.Application.Abstractions.Authorization;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Context;

namespace NovaCore.Auth.Application.Features.Accounts.Commands.SetAccountLevel;

public sealed class SetAccountLevelHandler(
    ICurrentUserService currentUserService,
    IAccountAuthorizationService accountAuthorizationService) : ICommandHandler<SetAccountLevelCommand>
{
    public async Task Handle(SetAccountLevelCommand request, CancellationToken ct = default)
    {
        var actorId = currentUserService.GetUserId() ?? throw new UnauthorizedException();
        var tenantId = RequestContext.Current.TenantId ?? Guid.Empty;

        await accountAuthorizationService.SetLevelAsync(actorId, request.AccountId, request.Level, tenantId, ct);
    }
}
