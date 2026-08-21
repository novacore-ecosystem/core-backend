using FluentValidation;

namespace NovaCore.Chat.Application.Features.ConversationHandovers.Commands.AcceptHandoverInvitation;

public sealed class AcceptHandoverInvitationValidator : AbstractValidator<AcceptHandoverInvitationCommand>
{
    public AcceptHandoverInvitationValidator()
    {
        RuleFor(x => x.TransferRequestId).NotEmpty();
    }
}
