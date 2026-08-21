using FluentValidation;

using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Chat.Application.Features.ConversationReasonSuggestions.Commands.TranslateConversationReasonSuggestion;

public sealed class TranslateConversationReasonSuggestionValidator : AbstractValidator<TranslateConversationReasonSuggestionCommand>
{
    public TranslateConversationReasonSuggestionValidator()
    {
        RuleFor(x => x.ConversationReasonSuggestionId).NotEmpty();

        RuleFor(x => x.LanguageCode)
            .Must(LanguageCode.IsValid).WithMessage("Language code is not supported.");

        RuleFor(x => x.Text)
            .Must(ConversationReasonSuggestion.IsValidText).WithMessage("Text is required.")
            .MaximumLength(200).WithMessage("Text must not exceed 200 characters.");
    }
}
