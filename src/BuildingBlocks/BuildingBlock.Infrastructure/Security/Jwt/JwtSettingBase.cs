using NovaCore.BuildingBlock.Infrastructure.Configurations;

namespace NovaCore.BuildingBlock.Infrastructure.Security.Jwt;

/// <summary>Baseline JWT signing shape shared by every service that issues its own tokens; subclasses just bind their own section.</summary>
public abstract class JwtSettingBase : ISetting
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
