using NovaCore.Auth.Application.Configurations;

using FluentValidation;

namespace NovaCore.Auth.Infrastructure.Configurations.Validators;

/// <summary>
/// Fail-fast rules for <see cref="RootSetting"/>. It lives in Auth.Application (Application-layer
/// code needs to read it directly, e.g. RefreshTokenHandler's Root bypass) and isn't an ISetting -
/// it can't join this assembly's reflection scan since it's declared in a different assembly - so
/// ConfigurationExtensions runs this validator manually against the same section, the same way it
/// already does for the external KafkaOptions.
/// </summary>
public sealed class RootSettingValidator : AbstractValidator<RootSetting>
{
    public RootSettingValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("RootId is required.");
        RuleFor(x => x.Username).NotEmpty().WithMessage("RootUsername is required.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("RootEmail must be a valid, non-empty email address.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("RootPassword is required.");
    }
}
