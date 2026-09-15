namespace NovaCore.Auth.Application.Configurations;

/// <summary>
/// The subset of Auth's JWT configuration (refresh-token lifetime) that Application-layer code
/// needs directly - e.g. OnAuthenticationSucceededHandler ties a new Session's ExpiresAt to the
/// same lifetime Login/RefreshToken already issue refresh tokens for. Lives in Application (not
/// Auth.Infrastructure, where the full AuthJwtSetting/JwtSettingBase live) for the same reason
/// RootSetting does - see RootSetting's own doc comment - since Application must not depend on
/// Auth.Infrastructure. Bound from the same "Jwt" configuration section AuthJwtSetting binds (see
/// Auth.Infrastructure's ConfigurationExtensions.AddAuthConfigurations), so the two never drift.
/// </summary>
public sealed class SessionJwtSetting
{
    public const string Section = "Jwt";

    public int RefreshTokenExpirationDays { get; init; } = 7;
}
