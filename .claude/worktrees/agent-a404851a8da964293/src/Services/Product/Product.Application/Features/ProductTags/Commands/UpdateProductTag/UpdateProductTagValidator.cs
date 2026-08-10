using FluentValidation;

namespace NovaCore.Product.Application.Features.ProductTags.Commands.UpdateProductTag;

public sealed class UpdateProductTagValidator : AbstractValidator<UpdateProductTagCommand>
{
    public UpdateProductTagValidator()
    {
        RuleFor(x => x.ProductTagId).NotEmpty().WithMessage("ProductTagId is required");

        RuleFor(x => x.Name)
            .Must(ProductTag.IsValidName)
            .WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
    }
}
