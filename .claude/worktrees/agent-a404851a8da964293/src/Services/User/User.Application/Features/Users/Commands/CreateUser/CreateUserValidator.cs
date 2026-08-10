using FluentValidation;

using NovaCore.User.Application.Common.Regex;

namespace NovaCore.User.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("UserName is required")
            .Length(3, 50).WithMessage("UserName must be between 3 and 50 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("PhoneNumber is required")
            .Matches(PhoneNumberPattern.Value()).WithMessage("PhoneNumber must contain at least 10 digits");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required")
            .Length(1, 50).WithMessage("FirstName must be between 1 and 50 characters");

        RuleFor(x => x.MiddleName)
            .MaximumLength(50).WithMessage("MiddleName must be at most 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required")
            .Length(1, 50).WithMessage("LastName must be between 1 and 50 characters");

        // Valid role names are Auth's own Identity roles (AccountRole/Role.Name) - User service has
        // no local catalog to validate against until the cross-service Role/Position admin
        // provisioning workflow exists (see CreateUserCommand). Shape-only validation for now.
        RuleFor(x => x.Roles)
            .NotEmpty().WithMessage("Roles is required");

        RuleFor(x => x.TempPassword)
            .MinimumLength(6).WithMessage("TempPassword must be at least 6 characters")
            .When(x => !string.IsNullOrEmpty(x.TempPassword));
    }
}
