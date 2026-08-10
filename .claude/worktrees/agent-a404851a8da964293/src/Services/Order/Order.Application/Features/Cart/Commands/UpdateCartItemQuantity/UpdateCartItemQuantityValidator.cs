using FluentValidation;

namespace NovaCore.Order.Application.Features.Cart.Commands.UpdateCartItemQuantity;

public sealed class UpdateCartItemQuantityValidator : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityValidator()
    {
        RuleFor(x => x.VariationId)
            .NotEmpty().WithMessage("VariationId is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0 - remove the item instead of setting it to zero or less");
    }
}
