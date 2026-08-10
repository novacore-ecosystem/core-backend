using FluentValidation;

namespace NovaCore.Auth.Application.Features.Auth.Commands.Register;

public sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters.")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

        RuleFor(x => x.MiddleName)
            .MaximumLength(50).WithMessage("Middle name must not exceed 50 characters.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(10).WithMessage("Phone number must be at least 10 characters.")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.");
    }
}
