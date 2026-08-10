using FluentValidation;

namespace NovaCore.Inventory.Application.Features.Inventories.Commands.CycleCount;

public sealed class CompleteCycleCountValidator : AbstractValidator<CompleteCycleCountCommand>
{
    public CompleteCycleCountValidator()
    {
        RuleFor(x => x.CountId)
            .NotEmpty()
            .WithMessage("Cycle count ID is required.");

        RuleFor(x => x.CountedItems)
            .NotEmpty()
            .WithMessage("At least one item must be counted.");

        RuleForEach(x => x.CountedItems)
            .SetValidator(new CycleCountItemValidator());

        RuleFor(x => x.VarianceThresholdPercent)
            .GreaterThan(0)
            .WithMessage("Variance threshold must be greater than 0.");

        RuleFor(x => x.VarianceThresholdPercent)
            .LessThanOrEqualTo(100)
            .WithMessage("Variance threshold must not exceed 100 percent.");
    }
}

public sealed class CycleCountItemValidator : AbstractValidator<CycleCountItemRequest>
{
    public CycleCountItemValidator()
    {
        RuleFor(x => x.VariantId)
            .NotEmpty()
            .WithMessage("Product variant ID is required.");

        RuleFor(x => x.ActualQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Actual quantity must be 0 or greater.");
    }
}
