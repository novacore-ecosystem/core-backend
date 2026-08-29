using NovaCore.BuildingBlock.Infrastructure.Security.Jwt;

namespace NovaCore.Auth.Infrastructure.Configurations.Settings;

/// <summary>Signing configuration Auth uses to issue its own access and refresh tokens.</summary>
public sealed class AuthJwtSetting : JwtSettingBase
{
    public const string Section = "Jwt";
}
