using FluentValidation;

namespace NovaCore.Auth.Application.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            // TODO: confirm password policy rules — no existing policy validator found in the
            // codebase; using RegisterValidator's 8-character minimum as the closest existing
            // precedent until a real policy is defined.
            .MinimumLength(8).WithMessage("New password must be at least 8 characters.")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from the current password.");
    }
}
