using FluentValidation;

namespace NovaCore.Content.Application.Features.Contents.Commands.CreateContent;

public sealed class CreateContentValidator : AbstractValidator<CreateContentCommand>
{
    public CreateContentValidator()
    {
        RuleFor(x => x.ContentTypeId).NotEmpty();

        RuleFor(x => x.Slug)
            .Must(ContentSlug.IsValid)
            .WithMessage("Slug must be 1-200 characters, lowercase kebab-case");

        RuleFor(x => x.Title)
            .Must(ContentVersion.IsValidTitle)
            .WithMessage("Title is required")
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters");

        RuleFor(x => x.Summary)
            .MaximumLength(1000).WithMessage("Summary must not exceed 1000 characters");

        RuleFor(x => x.CreatedBy).NotEmpty();

        RuleFor(x => x.Visibility).IsInEnum();
    }
}
