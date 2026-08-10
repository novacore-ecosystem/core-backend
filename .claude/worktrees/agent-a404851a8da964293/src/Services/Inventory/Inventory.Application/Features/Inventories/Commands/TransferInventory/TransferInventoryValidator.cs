using FluentValidation;

namespace NovaCore.Inventory.Application.Features.Inventories.Commands.TransferInventory;

public sealed class TransferInventoryValidator : AbstractValidator<TransferInventoryCommand>
{
    public TransferInventoryValidator()
    {
        RuleFor(x => x.SourceWarehouseId)
            .NotEmpty()
            .WithMessage("Source warehouse ID is required.");

        RuleFor(x => x.DestinationWarehouseId)
            .NotEmpty()
            .WithMessage("Destination warehouse ID is required.");

        RuleFor(x => x.SourceWarehouseId)
            .NotEqual(x => x.DestinationWarehouseId)
            .WithMessage("Source and destination warehouse must be different.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one item must be transferred.");

        RuleForEach(x => x.Items)
            .SetValidator(new TransferInventoryItemValidator());

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Transfer reason is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("Reason must not exceed 500 characters.");
    }
}

public sealed class TransferInventoryItemValidator : AbstractValidator<TransferInventoryItem>
{
    public TransferInventoryItemValidator()
    {
        RuleFor(x => x.VariantId)
            .NotEmpty()
            .WithMessage("Product variant ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0.");
    }
}
