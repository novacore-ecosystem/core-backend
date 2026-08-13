using System.Text.Json;

using NovaCore.Auth.Domain.Entities.Tenants;

namespace NovaCore.Auth.Application.Common;

/// <summary>Effective (fallback merged with this language's override) view for one supported
/// non-default language - see docs/services/auth-service.md, "Merged Tenant Translations". Shared
/// between GetTenantQuery (detail) and GetTenantBootstrapQuery (bootstrap) - both need the exact
/// same computation.</summary>
public sealed record EffectiveTranslationResponse(
    JsonElement Configuration,
    JsonElement Dictionary);

internal static class TenantTranslationMerger
{
    /// <summary>The fallback resource (null LanguageCode) itself is never exposed as an entry,
    /// since it has no language code to key it by - each entry here is fallback merged with that
    /// language's override, override wins on conflicting keys.</summary>
    public static Dictionary<string, EffectiveTranslationResponse> BuildEffective(
        Tenant tenant,
        IReadOnlyList<string> supportedLanguages)
    {
        var fallback = tenant.Locales.FirstOrDefault(l => l.LanguageCode is null);
        var fallbackConfigJson = fallback?.ConfigurationJson ?? "{}";
        var fallbackDictionaryJson = fallback?.DictionaryJson ?? "{}";

        var result = new Dictionary<string, EffectiveTranslationResponse>(StringComparer.Ordinal);

        foreach (var language in supportedLanguages)
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
}
