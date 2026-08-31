using System.Text.Json;

using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Contract.Events.Tenant;
using NovaCore.BuildingBlock.SharedKernel.Context;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantDictionary;

public sealed class UpdateTenantDictionaryHandler(
    ITenantWriteService tenantWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore) : ICommandHandler<UpdateTenantDictionaryCommand>
{
    public async Task Handle(UpdateTenantDictionaryCommand request, CancellationToken ct = default)
    {
        if (request.Dictionary.ValueKind != JsonValueKind.Object)
            throw new BadRequestException("Dictionary payload must be a JSON object.");

        var patchJson = request.Dictionary.GetRawText();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var updatedTenant = await tenantWriteService.UpsertLocaleAsync(
                request.TenantId,
                request.Language,
                configurationJson: null,
                dictionaryJson: patchJson);

            var tenantVersionChangeEvent = new TenantVersionChangedIntegrationEvent(
                request.TenantId,
                updatedTenant.Version,
                RequestContext.Current.CorrelationId);
            await outboxStore.EnqueueAsync(tenantVersionChangeEvent, ct);
        }, ct: ct);
    }
}
