using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.DeleteTenant;

/// <summary>Soft delete only (Tenant.Delete(), ISoftDeleteEntity) - never physically removes the
/// row. A soft-deleted tenant drops out of every normal query via the global query filter, so a
/// repeat delete on an already-deleted tenant surfaces as NotFoundException, same as any other
/// operation against a deleted tenant - not treated as a special idempotent case.</summary>
public sealed class DeleteTenantHandler(ITenantWriteService tenantWriteService) : ICommandHandler<DeleteTenantCommand>
{
    public async Task Handle(DeleteTenantCommand request, CancellationToken ct = default)
    {
        await tenantWriteService.UpdateAsync(request.Id, tenant => tenant.Delete(), ct);
    }
}
