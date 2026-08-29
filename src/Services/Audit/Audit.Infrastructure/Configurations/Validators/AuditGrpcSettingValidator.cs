using NovaCore.Audit.Infrastructure.Configurations.Settings;

using FluentValidation;

namespace NovaCore.Audit.Infrastructure.Configurations.Validators;

public sealed class AuditGrpcSettingValidator : AbstractValidator<AuditGrpcSetting>
{
    public AuditGrpcSettingValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Url is required.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).WithMessage("Url must be an absolute URI.");
    }
}
