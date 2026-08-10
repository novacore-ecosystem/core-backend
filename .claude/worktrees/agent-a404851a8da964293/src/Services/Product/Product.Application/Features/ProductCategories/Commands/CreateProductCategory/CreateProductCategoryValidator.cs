using FluentValidation;

using NovaCore.Product.Domain.ValueObjects;

namespace NovaCore.Product.Application.Features.ProductCategories.Commands.CreateProductCategory;

public sealed class CreateProductCategoryValidator : AbstractValidator<CreateProductCategoryCommand>
{
    public CreateProductCategoryValidator()
    {
        RuleFor(x => x.Code)
            .Must(CategoryCode.IsValid)
            .WithMessage("Code must be 1-50 characters and contain only letters, digits, and hyphens");

        RuleFor(x => x.Name)
            .Must(ProductCategory.IsValidName)
            .WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
    }
}
