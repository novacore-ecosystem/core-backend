using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationTransferRequests;

namespace NovaCore.Chat.Application.Features.ConversationHandovers.Commands.RejectHandoverInvitation;

public sealed class RejectHandoverInvitationHandler(
    IConversationTransferRequestReadService transferRequestReadService,
    IConversationTransferRequestWriteService transferRequestWriteService,
    ICurrentUserService currentUser) : ICommandHandler<RejectHandoverInvitationCommand>
{
    public async Task Handle(RejectHandoverInvitationCommand request, CancellationToken ct = default)
    {
        var transferRequest = await transferRequestReadService.GetByIdAsync(request.TransferRequestId, ct)
            ?? throw new NotFoundException(nameof(ConversationTransferRequest), request.TransferRequestId);

        var callerId = currentUser.GetUserId() ?? throw new UnauthorizedException();
        if (transferRequest.ToUserId != callerId)
            throw new ForbiddenException("This handover invitation was not addressed to you.");

        await transferRequestWriteService.RejectAsync(transferRequest.Id, ct);
    }
}
