using NovaCore.Product.Infrastructure.Configurations.Settings;

using FluentValidation;

namespace NovaCore.Product.Infrastructure.Configurations.Validators;

public sealed class ProductGrpcSettingValidator : AbstractValidator<ProductGrpcSetting>
{
    public ProductGrpcSettingValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Url is required.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).WithMessage("Url must be an absolute URI.");
    }
}
