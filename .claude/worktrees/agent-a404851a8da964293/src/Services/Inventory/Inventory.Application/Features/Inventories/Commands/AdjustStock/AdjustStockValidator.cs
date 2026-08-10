using FluentValidation;

namespace NovaCore.Inventory.Application.Features.Inventories.Commands.AdjustStock;

public sealed class AdjustStockValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockValidator()
    {
        RuleFor(x => x.InventoryId)
            .NotEmpty().WithMessage("InventoryId is required");

        RuleFor(x => x.NewQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("NewQuantity cannot be negative");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");
    }
}
