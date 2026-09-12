using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

using NovaCore.BuildingBlock.Application.Exceptions;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantPermissionBoundary;

public sealed class UpdateTenantPermissionBoundaryHandler(
    ITenantReadService tenantReadService,
    ITenantWriteService tenantWriteService) : ICommandHandler<UpdateTenantPermissionBoundaryCommand>
{
    public async Task Handle(UpdateTenantPermissionBoundaryCommand request, CancellationToken ct = default)
    {
        var tenant = await tenantReadService.GetByIdAsync(request.TenantId, ct)
            ?? throw new NotFoundException("Tenant", request.TenantId);

        var metadata = tenant.Metadata;
        metadata.PermissionBoundaryEnabled = request.Enabled;

        await tenantWriteService.UpdateAsync(request.TenantId, t => t.UpdateMetadata(metadata), ct);
    }
}
