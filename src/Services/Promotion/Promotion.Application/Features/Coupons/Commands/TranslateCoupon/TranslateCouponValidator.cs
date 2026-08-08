using FluentValidation;

namespace NovaCore.Promotion.Application.Features.Coupons.Commands.TranslateCoupon;

public sealed class TranslateCouponValidator : AbstractValidator<TranslateCouponCommand>
{
    public TranslateCouponValidator()
    {
        RuleFor(x => x.CouponId)
            .NotEmpty().WithMessage("CouponId is required");

        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("LanguageCode is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");
    }
}
