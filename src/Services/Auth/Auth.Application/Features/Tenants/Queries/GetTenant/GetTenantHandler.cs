using System.Text.Json;

using NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;
using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Common;
using NovaCore.Auth.Domain.Entities.TenantClients;
using NovaCore.Auth.Domain.Entities.Tenants;

using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.Application.Features.Tenants.Queries.GetTenant;

public sealed class GetTenantHandler(
    ITenantReadService tenantReadService,
    ITenantClientReadService tenantClientReadService) : IQueryHandler<GetTenantQuery, TenantDetailResponse>
{
    public async Task<TenantDetailResponse> Handle(GetTenantQuery request, CancellationToken ct = default)
    {
        var tenant = await tenantReadService.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Tenant", request.Id);

        var clients = await tenantClientReadService.ListByTenantAsync(request.Id, ct);

        return new TenantDetailResponse(
            tenant.Id,
            tenant.Code.Value,
            tenant.Name,
            tenant.LogoUrl,
            tenant.FaviconUrl,
            tenant.IsActive,
            tenant.Version,
            [.. tenant.Locales.Select(ToLocaleResponse)],
            BuildEffectiveTranslations(tenant),
            LanguageCodeConstant.SupportedLanguages,
            [.. clients.Select(ToClientSummary)],
            tenant.CreatedAt,
            tenant.UpdatedAt);
    }

    private static TenantLocaleResponse ToLocaleResponse(TenantLocale locale) => new(
        locale.LanguageCode?.Value,
        ParseJson(locale.ConfigurationJson),
        ParseJson(locale.DictionaryJson));

    /// <summary>Effective view per supported non-default language - fallback (null LanguageCode)
    /// merged with that language's override, override wins. The fallback resource itself never
    /// appears as an entry, since it has no language code to key it by (see docs/services/
    /// auth-service.md, "Merged Tenant Translations").</summary>
    private static Dictionary<string, EffectiveTranslationResponse> BuildEffectiveTranslations(Tenant tenant)
    {
        var fallback = tenant.Locales.FirstOrDefault(l => l.LanguageCode is null);
        var fallbackConfigJson = fallback?.ConfigurationJson ?? "{}";
        var fallbackDictionaryJson = fallback?.DictionaryJson ?? "{}";

        var result = new Dictionary<string, EffectiveTranslationResponse>(StringComparer.Ordinal);

        foreach (var language in LanguageCodeConstant.SupportedLanguages)
        {
            var overrideLocale = tenant.Locales.FirstOrDefault(l => l.LanguageCode?.Value == language);

            var mergedConfig = JsonMergeHelper.Merge(fallbackConfigJson, overrideLocale?.ConfigurationJson);
            var mergedDictionary = JsonMergeHelper.Merge(fallbackDictionaryJson, overrideLocale?.DictionaryJson);

            result[language] = new EffectiveTranslationResponse(
                mergedConfig.Deserialize<JsonElement>(),
                mergedDictionary.Deserialize<JsonElement>());
        }

        return result;
    }

    private static TenantClientSummaryResponse ToClientSummary(TenantClient client) => new(
        client.Id,
        client.Name,
        client.PublicKey.Value,
        client.Status.ToString(),
        client.ExpiresAt,
        client.RevokedAt);

    private static JsonElement ParseJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
