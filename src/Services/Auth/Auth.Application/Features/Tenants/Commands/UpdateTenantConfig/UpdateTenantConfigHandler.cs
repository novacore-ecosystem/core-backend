using System.Text.Json;

using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Common;

using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantConfig;

public sealed class UpdateTenantConfigHandler(ITenantWriteService tenantWriteService)
    : ICommandHandler<UpdateTenantConfigCommand>
{
    public async Task Handle(UpdateTenantConfigCommand request, CancellationToken ct = default)
    {
        if (request.Config.ValueKind != JsonValueKind.Object)
            throw new BadRequestException("Config payload must be a JSON object.");

        var language = request.Language is null ? null : LanguageCode.Create(request.Language);
        var patchJson = request.Config.GetRawText();

        await tenantWriteService.UpdateWithLocalesAsync(request.TenantId, tenant =>
        {
            var existing = tenant.Locales.FirstOrDefault(l => l.LanguageCode == language);
            var configurationJson = existing?.ConfigurationJson ?? "{}";
            var dictionaryJson = existing?.DictionaryJson ?? "{}";

            var mergedConfig = JsonMergeHelper.Merge(configurationJson, patchJson);

            tenant.SetLocale(language, mergedConfig.ToJsonString(), dictionaryJson);
            tenant.IncrementVersion();
        }, ct);
    }
}
