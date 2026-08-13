using System.Text.Json.Nodes;

using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Common;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Contract.Events.Tenant;
using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.UpsertTenantTranslation;

public sealed class UpsertTenantTranslationHandler(
    ITenantWriteService tenantWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUserService) : ICommandHandler<UpsertTenantTranslationCommand>
{
    public async Task Handle(UpsertTenantTranslationCommand request, CancellationToken ct = default)
    {
        var language = LanguageCode.Create(request.Language);
        var patch = new JsonObject { [request.Key] = JsonValue.Create(request.Value) };
        var newVersion = 0;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await tenantWriteService.UpdateWithLocalesAsync(request.TenantId, tenant =>
            {
                var existing = tenant.Locales.FirstOrDefault(l => l.LanguageCode == language);
                var configurationJson = existing?.ConfigurationJson ?? "{}";
                var dictionaryJson = existing?.DictionaryJson ?? "{}";

                var mergedDictionary = JsonMergeHelper.Merge(dictionaryJson, patch.ToJsonString());

                tenant.SetLocale(language, configurationJson, mergedDictionary.ToJsonString());
                tenant.IncrementVersion();
                newVersion = tenant.Version;
            }, ct);

            await outboxStore.EnqueueAsync(
                new TenantVersionChangedIntegrationEvent(request.TenantId, newVersion, currentUserService.GetCorrelationId()),
                ct);
        }, ct: ct);
    }
}
