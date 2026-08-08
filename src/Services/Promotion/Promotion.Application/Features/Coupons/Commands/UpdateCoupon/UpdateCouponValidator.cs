using FluentValidation;

namespace NovaCore.Promotion.Application.Features.Coupons.Commands.UpdateCoupon;

public sealed class UpdateCouponValidator : AbstractValidator<UpdateCouponCommand>
{
    public UpdateCouponValidator()
    {
        RuleFor(x => x.CouponId)
            .NotEmpty().WithMessage("CouponId is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("TimeZone is required");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("EndTime must be after StartTime");
    }
}
