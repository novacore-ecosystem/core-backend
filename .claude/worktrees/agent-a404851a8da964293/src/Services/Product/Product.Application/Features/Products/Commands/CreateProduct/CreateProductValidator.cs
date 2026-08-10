using FluentValidation;

namespace NovaCore.Product.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Code)
            .Must(ProductCode.IsValid)
            .WithMessage("Code must be 1-50 characters and contain only letters, digits, and hyphens");

        RuleFor(x => x.Name)
            .Must(ProductEntity.IsValidName)
            .WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.Slug)
            .Must(Slug.IsValid)
            .WithMessage("Slug must be 1-200 characters, lowercase kebab-case");

        RuleFor(x => x.Variations)
            .NotEmpty().WithMessage("At least one variation is required");

        RuleFor(x => x.Variations)
            .Must(v => v.Count(i => i.IsDefault) <= 1)
            .WithMessage("At most one variation may be marked as Default")
            .When(x => x.Variations is { Count: > 0 });

        RuleFor(x => x.Variations)
            .Must(v => v.Select(i => i.Sku.Trim().ToUpperInvariant()).Distinct().Count() == v.Count)
            .WithMessage("Variation SKUs must be unique within the request")
            .When(x => x.Variations is { Count: > 0 });

        RuleForEach(x => x.Variations).ChildRules(variation =>
        {
            variation.RuleFor(v => v.Sku)
                .Must(Sku.IsValid)
                .WithMessage("Variation SKU must be 1-50 characters and contain only letters, digits, and hyphens");

            variation.RuleFor(v => v.Name)
                .Must(ProductVariant.IsValidName)
                .WithMessage("Variation name is required")
                .MaximumLength(200).WithMessage("Variation name must not exceed 200 characters");

            variation.RuleFor(v => v.Price)
                .Must(ProductVariant.IsValidPrice)
                .WithMessage("Variation price cannot be negative");

            variation.RuleFor(v => v.Cost)
                .Must(ProductVariant.IsValidCost)
                .WithMessage("Variation cost cannot be negative");

            variation.RuleFor(v => v.Weight)
                .Must(ProductVariant.IsValidWeight)
                .WithMessage("Variation weight must be greater than zero when specified");

            variation.RuleFor(v => v.Barcode)
                .Must(b => b is null || Barcode.IsValid(b))
                .WithMessage("Barcode must be 8-14 numeric digits (EAN/UPC/GTIN)");
        });
    }
}
