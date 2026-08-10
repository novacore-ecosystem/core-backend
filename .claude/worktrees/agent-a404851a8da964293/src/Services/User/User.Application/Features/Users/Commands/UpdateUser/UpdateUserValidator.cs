using FluentValidation;

using NovaCore.User.Application.Common.Regex;

namespace NovaCore.User.Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required")
            .Length(1, 50).WithMessage("FirstName must be between 1 and 50 characters");

        RuleFor(x => x.MiddleName)
            .MaximumLength(50).WithMessage("MiddleName must be at most 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required")
            .Length(1, 50).WithMessage("LastName must be between 1 and 50 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("PhoneNumber is required")
            .Matches(PhoneNumberPattern.Value()).WithMessage("PhoneNumber must contain at least 10 digits");
    }
}
