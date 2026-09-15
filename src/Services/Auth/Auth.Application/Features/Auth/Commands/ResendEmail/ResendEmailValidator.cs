using FluentValidation;

namespace NovaCore.Auth.Application.Features.Auth.Commands.ResendEmail;

public sealed class ResendEmailValidator : AbstractValidator<ResendEmailCommand>
{
    public ResendEmailValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("A valid email is required.");
        RuleFor(x => x.Purpose).IsInEnum().WithMessage("Purpose must be EmailVerification or PasswordReset.");
    }
}
