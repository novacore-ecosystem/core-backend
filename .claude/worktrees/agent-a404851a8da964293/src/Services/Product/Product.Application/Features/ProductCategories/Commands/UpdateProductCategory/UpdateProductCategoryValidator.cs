using FluentValidation;

namespace NovaCore.Product.Application.Features.ProductCategories.Commands.UpdateProductCategory;

public sealed class UpdateProductCategoryCommandValidator : AbstractValidator<UpdateProductCategoryCommand>
{
    public UpdateProductCategoryCommandValidator()
    {
        RuleFor(x => x.ProductCategoryId)
            .NotEmpty().WithMessage("ProductCategoryId is required");

        RuleFor(x => x.Name)
            .Must(ProductCategory.IsValidName)
            .WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x)
            .Must(x => x.ParentCategoryId != x.ProductCategoryId)
            .WithMessage("A category cannot be its own parent");
    }
}
