using FluentValidation;

namespace NovaCore.Inventory.Application.Features.Inventories.Commands.StockOut;

public sealed class StockOutValidator : AbstractValidator<StockOutCommand>
{
    public StockOutValidator()
    {
        RuleFor(x => x.InventoryId)
            .NotEmpty().WithMessage("InventoryId is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");
    }
}
