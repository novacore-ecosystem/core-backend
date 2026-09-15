namespace NovaCore.Notification.Infrastructure.Configurations.Settings;

/// <summary>
/// Resend credentials/default sender for Notification Service's email channel - the only place in
/// NovaCore that sends email. ApiKey comes from Vault/environment, never appsettings.json. Not an
/// ISetting - AddEmailChannel needs the bound value synchronously to configure the Resend SDK's
/// own DI registration, so ConfigurationExtensions binds and validates it manually against the
/// same section (mirrors KafkaOptionsValidator's reasoning).
/// </summary>
public sealed class ResendMailSetting
{
    public const string Section = "Mail:Resend";

    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}
