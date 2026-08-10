using FluentValidation;

namespace NovaCore.Inventory.Application.Features.Inventories.Commands.CycleCount;

public sealed class StartCycleCountValidator : AbstractValidator<StartCycleCountCommand>
{
    public StartCycleCountValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty()
            .WithMessage("Warehouse ID is required.");

        RuleFor(x => x.CountDate)
            .NotEqual(default(DateTime))
            .WithMessage("Count date is required.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters.");
    }
}
