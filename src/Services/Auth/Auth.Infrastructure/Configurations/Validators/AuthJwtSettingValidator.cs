using NovaCore.Auth.Infrastructure.Configurations.Settings;

using NovaCore.BuildingBlock.Infrastructure.Security.Jwt;

namespace NovaCore.Auth.Infrastructure.Configurations.Validators;

/// <summary>Auth uses every inherited Jwt property, so it opts straight into the BuildingBlock's full baseline validation.</summary>
public sealed class AuthJwtSettingValidator : JwtSettingBaseValidator<AuthJwtSetting>;
