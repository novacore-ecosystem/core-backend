using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationAssignments;
using NovaCore.Chat.Application.Abstractions.Persistence.ConversationTransferRequests;

namespace NovaCore.Chat.Application.Features.ConversationHandovers.Commands.AcceptHandoverInvitation;

/// <summary>
/// Accepting is where ownership actually changes (see ConversationTransferRequest.cs's doc
/// comment) - releases the conversation's current active assignment (if any) and creates a new one
/// for the accepting user, alongside marking the transfer request Accepted, all in one transaction.
/// </summary>
public sealed class AcceptHandoverInvitationHandler(
    IConversationTransferRequestReadService transferRequestReadService,
    IConversationTransferRequestWriteService transferRequestWriteService,
    IConversationAssignmentReadService assignmentReadService,
    IConversationAssignmentWriteService assignmentWriteService,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork) : ICommandHandler<AcceptHandoverInvitationCommand>
{
    public async Task Handle(AcceptHandoverInvitationCommand request, CancellationToken ct = default)
    {
        var transferRequest = await transferRequestReadService.GetByIdAsync(request.TransferRequestId, ct)
            ?? throw new NotFoundException(nameof(ConversationTransferRequest), request.TransferRequestId);

        var callerId = currentUser.GetUserId() ?? throw new UnauthorizedException();
        if (transferRequest.ToUserId != callerId)
            throw new ForbiddenException("This handover invitation was not addressed to you.");

        var currentAssignment = await assignmentReadService.GetActiveByConversationIdAsync(transferRequest.ConversationId, ct);
        var newAssignment = ConversationAssignment.Create(transferRequest.ConversationId, callerId);

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await transferRequestWriteService.AcceptAsync(transferRequest.Id, ct);

            if (currentAssignment is not null)
                await assignmentWriteService.ReleaseAsync(currentAssignment.Id, ct);

            await assignmentWriteService.CreateAsync(newAssignment, ct);
        }, ct: ct);
    }
}
