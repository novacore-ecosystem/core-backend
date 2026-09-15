using FluentValidation;

namespace NovaCore.Auth.Application.Features.Auth.Commands.ConfirmEmail;

public sealed class ConfirmEmailValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty().WithMessage("AccountId is required.");
        RuleFor(x => x.Token).NotEmpty().WithMessage("Token is required.");
    }
}
