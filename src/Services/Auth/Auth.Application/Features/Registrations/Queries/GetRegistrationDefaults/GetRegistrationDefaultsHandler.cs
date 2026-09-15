using NovaCore.Auth.Application.Abstractions.Persistence.Apps;
using NovaCore.Auth.Application.Abstractions.Registrations;
using NovaCore.BuildingBlock.SharedKernel.Context;

namespace NovaCore.Auth.Application.Features.Registrations.Queries.GetRegistrationDefaults;

/// <summary>
/// Reads through <see cref="IRegistrationDefaultsCache"/> - the same reader RegisterHandler
/// consults - rather than querying Persistence directly, so the management screen and Register
/// never disagree about what's currently configured.
/// </summary>
public sealed class GetRegistrationDefaultsHandler(
    IAppReadService appReadService,
    IRegistrationDefaultsCache registrationDefaultsCache)
    : IQueryHandler<GetRegistrationDefaultsQuery, RegistrationDefaultsResponse>
{
    public async Task<RegistrationDefaultsResponse> Handle(
        GetRegistrationDefaultsQuery request,
        CancellationToken ct = default)
    {
        _ = await appReadService.GetByIdAsync(request.AppId, ct)
            ?? throw new NotFoundException("App", request.AppId);

        var tenantId = RequestContext.Current.TenantId ?? Guid.Empty;
        var defaults = await registrationDefaultsCache.GetAsync(tenantId, request.AppId, ct);

        return new RegistrationDefaultsResponse(defaults.RoleIds, defaults.PermissionKeys);
    }
}
