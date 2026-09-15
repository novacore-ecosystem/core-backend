using NovaCore.Notification.Infrastructure.Configurations.Settings;

using FluentValidation;

namespace NovaCore.Notification.Infrastructure.Configurations.Validators;

public sealed class ResendMailSettingValidator : AbstractValidator<ResendMailSetting>
{
    public ResendMailSettingValidator()
    {
        RuleFor(x => x.ApiKey).NotEmpty().WithMessage("ApiKey is required.");
        RuleFor(x => x.FromEmail).NotEmpty().EmailAddress().WithMessage("FromEmail must be a valid email address.");
    }
}
