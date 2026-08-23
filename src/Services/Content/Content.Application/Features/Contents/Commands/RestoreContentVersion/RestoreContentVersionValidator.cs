using FluentValidation;

namespace NovaCore.Content.Application.Features.Contents.Commands.RestoreContentVersion;

public sealed class RestoreContentVersionValidator : AbstractValidator<RestoreContentVersionCommand>
{
    public RestoreContentVersionValidator()
    {
        RuleFor(x => x.ContentId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.RestoredBy).NotEmpty();
    }
}
