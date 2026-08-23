using FluentValidation;

namespace NovaCore.Content.Application.Features.Contents.Commands.RestoreContent;

public sealed class RestoreContentValidator : AbstractValidator<RestoreContentCommand>
{
    public RestoreContentValidator()
    {
        RuleFor(x => x.ContentId).NotEmpty();
    }
}
