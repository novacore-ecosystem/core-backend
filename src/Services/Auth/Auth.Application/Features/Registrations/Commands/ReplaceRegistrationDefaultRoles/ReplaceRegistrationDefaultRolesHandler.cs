using NovaCore.Auth.Application.Abstractions.Persistence.Apps;
using NovaCore.Auth.Application.Abstractions.Persistence.Registrations;
using NovaCore.Auth.Application.Abstractions.Registrations;
using NovaCore.BuildingBlock.SharedKernel.Context;

namespace NovaCore.Auth.Application.Features.Registrations.Commands.ReplaceRegistrationDefaultRoles;

/// <summary>
/// Persists first, then refreshes the (Tenant, App) cache - never the reverse, so a concurrent
/// registration can never observe a refreshed cache ahead of the database write it was built from.
/// </summary>
public sealed class ReplaceRegistrationDefaultRolesHandler(
    IAppReadService appReadService,
    IRegistrationDefaultsWriteService registDefaultsWrite,
    IRegistrationDefaultsCache registDefaultsCache) : ICommandHandler<ReplaceRegistrationDefaultRolesCommand>
{
    public async Task Handle(
        ReplaceRegistrationDefaultRolesCommand request,
        CancellationToken ct = default)
    {
        _ = await appReadService.GetByIdAsync(request.AppId, ct)
            ?? throw new NotFoundException("App", request.AppId);

        var tenantId = RequestContext.Current.TenantId ?? Guid.Empty;
        await registDefaultsWrite.ReplaceRolesAsync(
            tenantId,
            request.AppId,
            request.RoleIds,
            ct);
        await registDefaultsCache.RefreshAsync(
            tenantId,
            request.AppId,
            ct);
    }
}
