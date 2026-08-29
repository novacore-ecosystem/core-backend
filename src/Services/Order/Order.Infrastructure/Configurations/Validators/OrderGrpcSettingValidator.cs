using NovaCore.Order.Infrastructure.Configurations.Settings;

using FluentValidation;

namespace NovaCore.Order.Infrastructure.Configurations.Validators;

public sealed class OrderGrpcSettingValidator : AbstractValidator<OrderGrpcSetting>
{
    public OrderGrpcSettingValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Url is required.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).WithMessage("Url must be an absolute URI.");
    }
}
