using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenant;

public sealed class UpdateTenantHandler(ITenantWriteService tenantWriteService) : ICommandHandler<UpdateTenantCommand>
{
    public async Task Handle(UpdateTenantCommand request, CancellationToken ct = default)
    {
        // Name/branding are bootstrap-relevant (see TenantBootstrapResponse) - bump Version so
        // connected clients know to reload (see docs/services/auth-service.md, "Versioning").
        await tenantWriteService.UpdateAsync(request.Id, tenant =>
        {
            tenant.Rename(request.Name);
            tenant.UpdateBranding(request.LogoUrl, request.FaviconUrl);
            tenant.IncrementVersion();
        }, ct);
    }
}
