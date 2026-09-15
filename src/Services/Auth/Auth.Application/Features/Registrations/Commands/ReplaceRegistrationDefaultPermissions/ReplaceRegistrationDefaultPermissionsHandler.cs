using NovaCore.Auth.Application.Abstractions.Persistence.Apps;
using NovaCore.Auth.Application.Abstractions.Persistence.Registrations;
using NovaCore.Auth.Application.Abstractions.Registrations;
using NovaCore.BuildingBlock.SharedKernel.Context;

namespace NovaCore.Auth.Application.Features.Registrations.Commands.ReplaceRegistrationDefaultPermissions;

/// <summary>
/// Persists first, then refreshes the (Tenant, App) cache - never the reverse, so a concurrent
/// registration can never observe a refreshed cache ahead of the database write it was built from.
/// </summary>
public sealed class ReplaceRegistrationDefaultPermissionsHandler(
    IAppReadService appReadService,
    IRegistrationDefaultsWriteService registDefaultWrite,
    IRegistrationDefaultsCache registDefaultCache) : ICommandHandler<ReplaceRegistrationDefaultPermissionsCommand>
{
    public async Task Handle(ReplaceRegistrationDefaultPermissionsCommand request, CancellationToken ct = default)
    {
        _ = await appReadService.GetByIdAsync(request.AppId, ct)
            ?? throw new NotFoundException("App", request.AppId);

        var tenantId = RequestContext.Current.TenantId ?? Guid.Empty;
        await registDefaultWrite.ReplacePermissionsAsync(
            tenantId,
            request.AppId,
            request.PermissionKeys,
            ct);
        await registDefaultCache.RefreshAsync(
            tenantId,
            request.AppId,
            ct);
    }
}
