using FluentValidation;

namespace NovaCore.Chat.Application.Features.ConversationHandovers.Commands.RejectHandoverInvitation;

public sealed class RejectHandoverInvitationValidator : AbstractValidator<RejectHandoverInvitationCommand>
{
    public RejectHandoverInvitationValidator()
    {
        RuleFor(x => x.TransferRequestId).NotEmpty();
    }
}
