using FluentValidation;

namespace NovaCore.Inventory.Application.Features.Inventories.Commands.ReceiveInventory;

public sealed class ReceiveInventoryValidator : AbstractValidator<ReceiveInventoryCommand>
{
    public ReceiveInventoryValidator()
    {
        RuleFor(x => x.PurchaseOrderNumber)
            .NotEmpty()
            .WithMessage("Purchase order number is required.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty()
            .WithMessage("Warehouse ID is required.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one item must be received.");

        RuleForEach(x => x.Items)
            .SetValidator(new ReceiveInventoryItemValidator());

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters.");
    }
}

public sealed class ReceiveInventoryItemValidator : AbstractValidator<ReceiveInventoryItem>
{
    public ReceiveInventoryItemValidator()
    {
        RuleFor(x => x.VariantId)
            .NotEmpty()
            .WithMessage("Product variant ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.LotNumber)
            .MaximumLength(100)
            .WithMessage("Lot number must not exceed 100 characters.");

        When(x => x.ManufactureDate.HasValue && x.ExpiryDate.HasValue, () =>
        {
            RuleFor(x => x.ManufactureDate)
                .LessThan(x => x.ExpiryDate)
                .WithMessage("Manufacture date must be before expiry date.");
        });
    }
}
