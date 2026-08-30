using System.Text.Json;

using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Auth.Domain.Entities.Tenants;

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
