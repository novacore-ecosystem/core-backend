using NovaCore.BuildingBlock.Infrastructure.Security.Jwt;

namespace NovaCore.Chat.Infrastructure.Configurations.Settings;

/// <summary>Signing configuration Chat uses to mint guest access tokens.</summary>
public sealed class ChatJwtSetting : JwtSettingBase
{
    public const string Section = "Jwt";
}
