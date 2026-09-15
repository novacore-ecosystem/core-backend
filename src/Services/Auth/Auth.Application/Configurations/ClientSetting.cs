namespace NovaCore.Auth.Application.Configurations;

/// <summary>
/// Frontend URLs Auth embeds into outbound emails (ForgotPassword, email verification) - each a
/// full URL Auth appends "?token=" to. Lives in Application (not Auth.Infrastructure) for the
/// same reason RootSetting/SessionJwtSetting do: ForgotPasswordHandler/RegisterHandler need it
/// directly and Application must never depend on Auth.Infrastructure.
/// </summary>
public sealed class ClientSetting
{
    public const string Section = "Client";

    public string ResetPasswordUrl { get; init; } = string.Empty;

    public string EmailVerificationUrl { get; init; } = string.Empty;
}
