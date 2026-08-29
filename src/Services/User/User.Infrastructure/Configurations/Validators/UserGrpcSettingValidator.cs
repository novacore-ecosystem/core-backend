using NovaCore.User.Infrastructure.Configurations.Settings;

using FluentValidation;

namespace NovaCore.User.Infrastructure.Configurations.Validators;

public sealed class UserGrpcSettingValidator : AbstractValidator<UserGrpcSetting>
{
    public UserGrpcSettingValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Url is required.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).WithMessage("Url must be an absolute URI.");
    }
}
