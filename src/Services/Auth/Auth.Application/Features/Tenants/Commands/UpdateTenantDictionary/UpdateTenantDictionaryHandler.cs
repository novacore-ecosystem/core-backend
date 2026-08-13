using System.Text.Json;

using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Common;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Contract.Events.Tenant;
using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantDictionary;

public sealed class UpdateTenantDictionaryHandler(
    ITenantWriteService tenantWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUserService) : ICommandHandler<UpdateTenantDictionaryCommand>
{
    public async Task Handle(UpdateTenantDictionaryCommand request, CancellationToken ct = default)
    {
        if (request.Dictionary.ValueKind != JsonValueKind.Object)
            throw new BadRequestException("Dictionary payload must be a JSON object.");

        var language = LanguageCode.Create(request.Language);
        var patchJson = request.Dictionary.GetRawText();
        var newVersion = 0;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await tenantWriteService.UpdateWithLocalesAsync(request.TenantId, tenant =>
            {
                var existing = tenant.Locales.FirstOrDefault(l => l.LanguageCode == language);
                var configurationJson = existing?.ConfigurationJson ?? "{}";
                var dictionaryJson = existing?.DictionaryJson ?? "{}";

                var mergedDictionary = JsonMergeHelper.Merge(dictionaryJson, patchJson);

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
