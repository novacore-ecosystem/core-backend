using FluentValidation;

namespace NovaCore.Order.Application.Features.Cart.Commands.AddCartItem;

public sealed class AddCartItemValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemValidator()
    {
        RuleFor(x => x.VariationId)
            .NotEmpty().WithMessage("VariationId is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");
    }
}
