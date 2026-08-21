using FluentValidation;

using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.Chat.Domain.Entities.Contacts;

namespace NovaCore.Chat.Application.Features.Conversations.Commands.StartGuestConversation;

public sealed class StartGuestConversationValidator : AbstractValidator<StartGuestConversationCommand>
{
    public StartGuestConversationValidator()
    {
        RuleFor(x => x.DisplayName)
            .Must(Contact.IsValidDisplayName).WithMessage("Display name is required")
            .MaximumLength(200).WithMessage("Display name must not exceed 200 characters");

        RuleFor(x => x.Email)
            .Must(Email.IsValid).WithMessage("Email is not valid")
            .When(x => x.Email is not null);

        RuleFor(x => x.Phone)
            .Must(PhoneNumber.IsValid).WithMessage("Phone number is not valid")
            .When(x => x.Phone is not null);

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");
    }
}
