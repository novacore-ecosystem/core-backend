using FluentValidation;

namespace NovaCore.Product.Application.Features.Products.Commands.AddVariation;

public sealed class AddVariationValidator : AbstractValidator<AddVariationCommand>
{
    public AddVariationValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required");

        RuleFor(x => x.VariationInput.Sku)
            .Must(Sku.IsValid)
            .WithMessage("Sku must be 1-50 characters and contain only letters, digits, and hyphens");

        RuleFor(x => x.VariationInput.Name)
            .Must(ProductVariant.IsValidName)
            .WithMessage("Variation name is required")
            .MaximumLength(200).WithMessage("Variation name must not exceed 200 characters");

        RuleFor(x => x.VariationInput.Price)
            .Must(ProductVariant.IsValidPrice)
            .WithMessage("Price cannot be negative");

        RuleFor(x => x.VariationInput.Cost)
            .Must(ProductVariant.IsValidCost)
            .WithMessage("Cost cannot be negative");

        RuleFor(x => x.VariationInput.Weight)
            .Must(ProductVariant.IsValidWeight)
            .WithMessage("Weight must be greater than zero when specified");

        RuleFor(x => x.VariationInput.Barcode)
            .Must(b => b is null || Barcode.IsValid(b))
            .WithMessage("Barcode must be 8-14 numeric digits (EAN/UPC/GTIN)");
    }
}
