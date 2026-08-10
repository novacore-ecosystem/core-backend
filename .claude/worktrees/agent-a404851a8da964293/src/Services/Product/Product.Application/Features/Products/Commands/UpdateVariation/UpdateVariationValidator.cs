using FluentValidation;

using NovaCore.Product.Domain.ValueObjects;

namespace NovaCore.Product.Application.Features.Products.Commands.UpdateVariation;

public sealed class UpdateVariationValidator : AbstractValidator<UpdateVariationCommand>
{
    public UpdateVariationValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required");
        RuleFor(x => x.VariationId).NotEmpty().WithMessage("VariationId is required");

        RuleFor(x => x.Sku)
            .Must(Sku.IsValid)
            .WithMessage("Sku must be 1-50 characters and contain only letters, digits, and hyphens");

        RuleFor(x => x.Name)
            .Must(ProductVariant.IsValidName)
            .WithMessage("Variation name is required")
            .MaximumLength(200).WithMessage("Variation name must not exceed 200 characters");

        RuleFor(x => x.Price)
            .Must(ProductVariant.IsValidPrice)
            .WithMessage("Price cannot be negative");

        RuleFor(x => x.Cost)
            .Must(ProductVariant.IsValidCost)
            .WithMessage("Cost cannot be negative");

        RuleFor(x => x.Weight)
            .Must(ProductVariant.IsValidWeight)
            .WithMessage("Weight must be greater than zero when specified");

        RuleFor(x => x.Barcode)
            .Must(b => b is null || Barcode.IsValid(b))
            .WithMessage("Barcode must be 8-14 numeric digits (EAN/UPC/GTIN)");

        RuleFor(x => x.Status)
            .Must(s => Enum.TryParse<VariantStatus>(s, ignoreCase: true, out _))
            .WithMessage("Status must be one of: Active, Inactive, Discontinued");
    }
}
