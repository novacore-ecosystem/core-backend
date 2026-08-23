using FluentValidation;

using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.Content.Application.Common;

namespace NovaCore.Content.Application.Features.Contents.Commands.CreateContent;

public sealed class CreateContentValidator : AbstractValidator<CreateContentCommand>
{
    public CreateContentValidator()
    {
        RuleFor(x => x.ContentTypeId).NotEmpty();

        RuleFor(x => x.Slug)
            .Must(ContentSlug.IsValid)
            .WithMessage("Slug must be 1-200 characters, lowercase kebab-case");

        RuleFor(x => x.Language)
            .Must(x => LanguageCode.IsValid(ContentLanguageDefaults.OrDefault(x)))
            .WithMessage("Language must be one of the supported language codes");

        RuleFor(x => x.Title)
            .Must(ContentLocalization.IsValidTitle)
            .WithMessage("Title is required")
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters");

        RuleFor(x => x.Summary)
            .MaximumLength(1000).WithMessage("Summary must not exceed 1000 characters");

        RuleFor(x => x.Body)
            .Must(ContentLocalization.IsValidBody)
            .WithMessage("Body must be a valid JSON document");

        RuleFor(x => x.CreatedBy).NotEmpty();

        RuleFor(x => x.Visibility).IsInEnum();
    }
}
