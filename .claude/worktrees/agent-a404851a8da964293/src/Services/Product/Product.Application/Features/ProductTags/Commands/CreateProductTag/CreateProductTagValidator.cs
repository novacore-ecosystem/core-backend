using FluentValidation;

using NovaCore.Product.Domain.ValueObjects;

namespace NovaCore.Product.Application.Features.ProductTags.Commands.CreateProductTag;

public sealed class CreateProductTagValidator : AbstractValidator<CreateProductTagCommand>
{
    public CreateProductTagValidator()
    {
        RuleFor(x => x.Code)
            .Must(TagCode.IsValid)
            .WithMessage("Code must be 1-50 characters and contain only letters, digits, and hyphens");

        RuleFor(x => x.Name)
            .Must(ProductTag.IsValidName)
            .WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
    }
}
