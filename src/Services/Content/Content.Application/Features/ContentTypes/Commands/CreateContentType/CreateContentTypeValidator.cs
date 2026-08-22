using FluentValidation;

namespace NovaCore.Content.Application.Features.ContentTypes.Commands.CreateContentType;

public sealed class CreateContentTypeValidator : AbstractValidator<CreateContentTypeCommand>
{
    public CreateContentTypeValidator()
    {
        RuleFor(x => x.Key)
            .Must(ContentKey.IsValid)
            .WithMessage("Key must be lowercase alphanumeric segments separated by dots, underscores, or hyphens");

        RuleFor(x => x.Name)
            .Must(ContentType.IsValidName)
            .WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");
    }
}
