using FluentValidation;

namespace NovaCore.Promotion.Application.Features.Promotions.Commands.UpdatePromotion;

public sealed class UpdatePromotionValidator : AbstractValidator<UpdatePromotionCommand>
{
    public UpdatePromotionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("TimeZone is required");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("EndTime must be after StartTime");
    }
}
