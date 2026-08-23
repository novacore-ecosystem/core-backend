using FluentValidation;

namespace NovaCore.Content.Application.Features.Contents.Commands.DeleteContent;

public sealed class DeleteContentValidator : AbstractValidator<DeleteContentCommand>
{
    public DeleteContentValidator()
    {
        RuleFor(x => x.ContentId).NotEmpty();
    }
}
