using FluentValidation;

namespace NovaCore.Chat.Application.Features.ConversationReasonSuggestions.Commands.CreateConversationReasonSuggestion;

public sealed class CreateConversationReasonSuggestionValidator : AbstractValidator<CreateConversationReasonSuggestionCommand>
{
    public CreateConversationReasonSuggestionValidator()
    {
        RuleFor(x => x.Code)
            .Must(EntityCode.IsValid).WithMessage("Code must be uppercase alphanumeric with underscores/hyphens only.");
    }
}
