using FluentValidation;

namespace NovaCore.Chat.Application.Features.ConversationQueues.Commands.ClaimConversation;

public sealed class ClaimConversationValidator : AbstractValidator<ClaimConversationCommand>
{
    public ClaimConversationValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
    }
}
