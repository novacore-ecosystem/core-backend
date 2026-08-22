using FluentValidation;

namespace NovaCore.Content.Application.Features.Contents.Commands.PublishContent;

public sealed class PublishContentValidator : AbstractValidator<PublishContentCommand>
{
    public PublishContentValidator()
    {
        RuleFor(x => x.ContentId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
    }
}
