using FluentValidation;

namespace NovaCore.Chat.Application.Features.Conversations.Commands.CloseConversation;

public sealed class CloseConversationValidator : AbstractValidator<CloseConversationCommand>
{
    public CloseConversationValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
    }
}
