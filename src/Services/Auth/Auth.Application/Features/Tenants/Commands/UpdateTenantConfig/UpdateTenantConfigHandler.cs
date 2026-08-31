using System.Text.Json;

using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Contract.Events.Tenant;
using NovaCore.BuildingBlock.SharedKernel.Context;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantConfig;

public sealed class UpdateTenantConfigHandler(
    IUnitOfWork unitOfWork,
    ITenantWriteService tenantWriteService,
    IOutboxStore outboxStore) : ICommandHandler<UpdateTenantConfigCommand>
{
    public async Task Handle(UpdateTenantConfigCommand request, CancellationToken ct = default)
    {
        if (request.Config.ValueKind != JsonValueKind.Object)
            throw new BadRequestException("Config payload must be a JSON object.");

        var patchJson = request.Config.GetRawText();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var updatedTenant = await tenantWriteService.UpsertLocaleAsync(
                request.TenantId,
                request.Language,
                patchJson);

            var tenantVersionChangeEvent = new TenantVersionChangedIntegrationEvent(
                request.TenantId,
                updatedTenant.Version,
                RequestContext.Current.CorrelationId);
            await outboxStore.EnqueueAsync(tenantVersionChangeEvent, ct);
        }, ct: ct);
    }
}
