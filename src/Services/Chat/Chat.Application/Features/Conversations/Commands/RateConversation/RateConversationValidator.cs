using FluentValidation;

namespace NovaCore.Chat.Application.Features.Conversations.Commands.RateConversation;

public sealed class RateConversationValidator : AbstractValidator<RateConversationCommand>
{
    public RateConversationValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();

        RuleFor(x => x.Stars)
            .Must(ConversationRating.IsValidStars).WithMessage("Stars must be between 0 and 5.");

        RuleFor(x => x.Review)
            .MaximumLength(2000).WithMessage("Review must not exceed 2000 characters.");
    }
}
