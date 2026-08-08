using FluentValidation;

namespace NovaCore.Promotion.Application.Features.Coupons.Commands.CreateCoupon;

public sealed class CreateCouponValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");
    }
}
