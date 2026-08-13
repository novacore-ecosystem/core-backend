using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.DisableTenant;

/// <summary>Deactivate() is already idempotent (a bare bool assignment) - calling this on an
/// already-disabled tenant is a harmless no-op, no pre-check needed. Does not bump Version: a
/// disabled tenant's bootstrap access is rejected outright (see GetTenantBootstrapHandler), not
/// served with different content, so there is nothing for a connected client to "reload".</summary>
public sealed class DisableTenantHandler(ITenantWriteService tenantWriteService) : ICommandHandler<DisableTenantCommand>
{
    public async Task Handle(DisableTenantCommand request, CancellationToken ct = default)
    {
        await tenantWriteService.UpdateAsync(request.Id, tenant => tenant.Deactivate(), ct);
    }
}
