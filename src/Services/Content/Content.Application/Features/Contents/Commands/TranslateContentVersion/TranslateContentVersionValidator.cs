using FluentValidation;

using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Content.Application.Features.Contents.Commands.TranslateContentVersion;

public sealed class TranslateContentVersionValidator : AbstractValidator<TranslateContentVersionCommand>
{
    public TranslateContentVersionValidator()
    {
        RuleFor(x => x.ContentId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();

        RuleFor(x => x.TargetLanguage)
            .Must(LanguageCode.IsValid)
            .WithMessage("Target language must be one of the supported language codes");

        RuleFor(x => x.Title)
            .Must(ContentLocalization.IsValidTitle)
            .WithMessage("Title is required")
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters");

        RuleFor(x => x.Summary)
            .MaximumLength(1000).WithMessage("Summary must not exceed 1000 characters");

        RuleFor(x => x.Body)
            .Must(ContentLocalization.IsValidBody)
            .WithMessage("Body must be a valid JSON document");

        RuleFor(x => x.TranslatedBy).NotEmpty();
    }
}
