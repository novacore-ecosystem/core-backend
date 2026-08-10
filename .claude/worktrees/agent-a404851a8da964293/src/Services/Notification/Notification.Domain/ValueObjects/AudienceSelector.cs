using System.Text.Json;

namespace NovaCore.Notification.Domain.ValueObjects;

/// <summary>
/// Describes a <see cref="Entities.NotificationGroup"/>'s target audience. ConfigJson is left
/// unstructured on purpose (roles list, explicit user id list, or a segment definition all have
/// different shapes, and the configuration volume is small) - see
/// docs on NotificationGroup for why this stays JSON instead of a normalized membership table.
/// </summary>
public sealed class AudienceSelector : ValueObject
{
    public AudienceType Type { get; }
    public string? ConfigJson { get; }

    private AudienceSelector(AudienceType type, string? configJson)
    {
        Type = type;
        ConfigJson = configJson;
    }

    public static bool IsValid(AudienceType type, string? configJson) => GetValidationError(type, configJson) is null;

    public static bool TryCreate(AudienceType type, string? configJson, out AudienceSelector? selector)
    {
        if (GetValidationError(type, configJson) is not null)
        {
            selector = null;
            return false;
        }

        selector = new AudienceSelector(type, configJson?.Trim());
        return true;
    }

    public static AudienceSelector Create(AudienceType type, string? configJson = null)
    {
        var error = GetValidationError(type, configJson);
        if (error is not null)
            throw error;

        return new AudienceSelector(type, configJson?.Trim());
    }

    private static InvalidArgumentException? GetValidationError(AudienceType type, string? configJson)
    {
        if (type == AudienceType.All)
            return null;

        if (string.IsNullOrWhiteSpace(configJson))
            return ExceptionFactory.RequiredField($"Audience configuration is required for type {type}.");

        try
        {
            using var _ = JsonDocument.Parse(configJson);
        }
        catch (JsonException)
        {
            return ExceptionFactory.InvalidFormat("Audience configuration must be well-formed JSON.");
        }

        return null;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return ConfigJson ?? string.Empty;
    }
}
