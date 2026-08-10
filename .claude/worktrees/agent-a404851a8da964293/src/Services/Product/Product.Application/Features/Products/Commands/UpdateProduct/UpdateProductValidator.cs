using FluentValidation;

namespace NovaCore.Product.Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");

        RuleFor(x => x.Name)
            .Must(ProductEntity.IsValidName)
            .WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.Slug)
            .Must(Slug.IsValid)
            .WithMessage("Slug must be 1-200 characters, lowercase kebab-case");
    }
}
