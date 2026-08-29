using FluentValidation;

namespace NovaCore.BuildingBlock.Infrastructure.Security.Jwt;

/// <summary>Full-strictness baseline rules for <see cref="JwtSettingBase"/>. A service validator inherits this to opt into strict validation, or skips it to write its own lightweight rules instead.</summary>
public abstract class JwtSettingBaseValidator<TSetting> : AbstractValidator<TSetting>
    where TSetting : JwtSettingBase
{
    protected JwtSettingBaseValidator()
    {
        RuleFor(x => x.SecretKey).MinimumLength(32).WithMessage("SecretKey must be at least 32 characters for HMACSHA256 signing.");
        RuleFor(x => x.Issuer).NotEmpty().WithMessage("Issuer is required.");
        RuleFor(x => x.Audience).NotEmpty().WithMessage("Audience is required.");
        RuleFor(x => x.AccessTokenExpirationMinutes).GreaterThan(0).WithMessage("AccessTokenExpirationMinutes must be positive.");
        RuleFor(x => x.RefreshTokenExpirationDays).GreaterThan(0).WithMessage("RefreshTokenExpirationDays must be positive.");
    }
}
