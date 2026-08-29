using NovaCore.Auth.Infrastructure.Configurations.Settings;

using FluentValidation;

namespace NovaCore.Auth.Infrastructure.Configurations.Validators;

public sealed class AuthGrpcSettingValidator : AbstractValidator<AuthGrpcSetting>
{
    public AuthGrpcSettingValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Url is required.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).WithMessage("Url must be an absolute URI.");
    }
}
