using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Contract.Events.Tenant;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenant;

public sealed class UpdateTenantHandler(
    ITenantWriteService tenantWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUserService) : ICommandHandler<UpdateTenantCommand>
{
    public async Task Handle(UpdateTenantCommand request, CancellationToken ct = default)
    {
        var newVersion = 0;

        // Name/branding are bootstrap-relevant (see TenantBootstrapResponse) - bump Version and
        // enqueue the change event atomically with it (see docs/services/auth-service.md,
        // "Versioning" / "Cache Refresh After Tenant Update").
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await tenantWriteService.UpdateAsync(request.Id, tenant =>
            {
                tenant.Rename(request.Name);
                tenant.UpdateBranding(request.LogoUrl, request.FaviconUrl);
                tenant.IncrementVersion();
                newVersion = tenant.Version;
            }, ct);

            await outboxStore.EnqueueAsync(
                new TenantVersionChangedIntegrationEvent(request.Id, newVersion, currentUserService.GetCorrelationId()),
                ct);
        }, ct: ct);
    }
}
