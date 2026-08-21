using FluentValidation;

namespace NovaCore.Chat.Application.Features.Conversations.Commands.CreateConversation;

public sealed class CreateConversationValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Lifecycle).IsInEnum();
        RuleFor(x => x.Priority).IsInEnum();

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");
    }
}
