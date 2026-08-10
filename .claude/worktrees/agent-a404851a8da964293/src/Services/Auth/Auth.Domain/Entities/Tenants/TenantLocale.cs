using System.Text.Json;

using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Auth.Domain.Entities.Tenants;

/// <summary>
/// Owned child of Tenant - the entire bootstrap resource set (runtime configuration +
/// translation dictionary) for one locale. A null LanguageCode identifies the fallback
/// resource, served when no locale-specific row matches. Both payloads are intentionally
/// schema-less opaque JSON - bootstrap resources are denormalized per-locale blobs, not
/// structured/queryable columns, so they are stored and validated as raw JSON text rather
/// than mapped through a strongly-typed metadata bag (see docs/services/auth-service.md).
/// </summary>
public sealed class TenantLocale : BaseEntity<Guid>, IAuditable
{
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = default!;
    public LanguageCode? LanguageCode { get; private set; }
    public string ConfigurationJson { get; private set; } = "{}";
    public string DictionaryJson { get; private set; } = "{}";

    private TenantLocale() { }

    internal static TenantLocale Create(
        Guid tenantId,
        LanguageCode? languageCode,
        string configurationJson,
        string dictionaryJson)
    {
        ValidateJson(configurationJson, "Configuration");
        ValidateJson(dictionaryJson, "Dictionary");

        return new TenantLocale
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            LanguageCode = languageCode,
            ConfigurationJson = configurationJson,
            DictionaryJson = dictionaryJson,
        };
    }

    internal void UpdateContent(string configurationJson, string dictionaryJson)
    {
        ValidateJson(configurationJson, "Configuration");
        ValidateJson(dictionaryJson, "Dictionary");

        ConfigurationJson = configurationJson;
        DictionaryJson = dictionaryJson;
    }

    public static bool IsValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            using var _ = JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static void ValidateJson(string json, string fieldLabel)
    {
        if (!IsValidJson(json))
            throw ExceptionFactory.InvalidFormat($"{fieldLabel} JSON must be well-formed.");
    }
}
