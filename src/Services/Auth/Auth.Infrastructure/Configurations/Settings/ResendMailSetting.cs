namespace NovaCore.Auth.Infrastructure.Configurations.Settings;

/// <summary>
/// Resend credentials/default sender for Auth's own transactional email (verification, and any
/// future auth-owned email). ApiKey comes from Vault/environment, never appsettings.json. Not an
/// ISetting - AddAuthMail needs the bound value synchronously to configure the Resend SDK's own
/// DI registration, so ConfigurationExtensions binds and validates it manually against the same
/// section, the same way Notification.Infrastructure's own ResendMailSetting does.
/// </summary>
public sealed class ResendMailSetting
{
    public const string Section = "Mail:Resend";

    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}
