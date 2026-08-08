using FluentValidation;

namespace NovaCore.Promotion.Application.Features.Promotions.Commands.CreatePromotion;

public sealed class CreatePromotionValidator : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required");

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("TimeZone is required");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("EndTime must be after StartTime");
    }
}
