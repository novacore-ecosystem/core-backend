using FluentValidation;

namespace NovaCore.Auth.Application.Features.Auth.Commands.ResetPasswordWithToken;

public sealed class ResetPasswordWithTokenValidator : AbstractValidator<ResetPasswordWithTokenCommand>
{
    public ResetPasswordWithTokenValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            // TODO: confirm password policy rules — no existing policy validator found in the
            // codebase; using RegisterValidator's 8-character minimum as the closest existing
            // precedent until a real policy is defined.
            .MinimumLength(8).WithMessage("New password must be at least 8 characters.");
    }
}
