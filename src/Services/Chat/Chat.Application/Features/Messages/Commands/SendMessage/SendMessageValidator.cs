using FluentValidation;

namespace NovaCore.Chat.Application.Features.Messages.Commands.SendMessage;

public sealed class SendMessageValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();

        RuleFor(x => x.ClientMessageId)
            .Must(Message.IsValidClientMessageId).WithMessage("Client message id is required.")
            .MaximumLength(100).WithMessage("Client message id must not exceed 100 characters.");

        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Format).IsInEnum();

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(8000).WithMessage("Content must not exceed 8000 characters.");
    }
}
