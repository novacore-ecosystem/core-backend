using FluentValidation;

namespace NovaCore.Promotion.Application.Features.Coupons.Commands.CreateCoupon;

public sealed class CreateCouponValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponValidator()
    {
        RuleFor(x => x.PromotionId)
            .NotEmpty().WithMessage("PromotionId is required");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("TimeZone is required");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("EndTime must be after StartTime");
    }
}
