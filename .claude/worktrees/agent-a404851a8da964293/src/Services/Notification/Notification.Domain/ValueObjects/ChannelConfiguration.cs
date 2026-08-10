using System.Text.Json;

namespace NovaCore.Notification.Domain.ValueObjects;

/// <summary>
/// Raw JSON runtime configuration for one <see cref="Entities.NotificationChannel"/> (SMTP
/// host/port/credentials, a Telegram bot token, a Facebook page token, ...). Kept as opaque JSON
/// rather than a per-provider typed shape - the set of providers is open-ended and each has a
/// completely different config shape, so a strongly-typed VO per provider would multiply without
/// bound. Only structural validity (well-formed JSON) is enforced here; provider-specific
/// validation (e.g. "is this actually a working SMTP config") is Infrastructure's job via
/// <see cref="Entities.NotificationChannel.RecordValidationResult"/>.
/// </summary>
public sealed class ChannelConfiguration : ValueObject
{
    public string ConfigJson { get; }

    private ChannelConfiguration(string configJson)
    {
        ConfigJson = configJson;
    }

    public static ChannelConfiguration Empty { get; } = new("{}");

    public static bool IsValid(string? configJson) => GetValidationError(configJson) is null;

    public static bool TryCreate(string? configJson, out ChannelConfiguration? configuration)
    {
        if (GetValidationError(configJson) is not null)
        {
            configuration = null;
            return false;
        }

        configuration = new ChannelConfiguration(configJson!.Trim());
        return true;
    }

    public static ChannelConfiguration Create(string configJson)
    {
        var error = GetValidationError(configJson);
        if (error is not null)
            throw error;

        return new ChannelConfiguration(configJson.Trim());
    }

    private static InvalidArgumentException? GetValidationError(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
            return ExceptionFactory.RequiredField("Channel configuration cannot be empty.");

        try
        {
            using var _ = JsonDocument.Parse(configJson);
        }
        catch (JsonException)
        {
            return ExceptionFactory.InvalidFormat("Channel configuration must be well-formed JSON.");
        }

        return null;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return ConfigJson;
    }
}
