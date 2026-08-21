using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationAssignments;
using NovaCore.Chat.Application.Abstractions.Persistence.ConversationQueues;
using NovaCore.Chat.Application.Abstractions.Persistence.Conversations;
using NovaCore.Chat.Application.Common;

namespace NovaCore.Chat.Application.Features.ConversationQueues.Commands.ClaimConversation;

/// <summary>
/// "Discover/pick" from the queue - spans ConversationQueueItem, ConversationAssignment and
/// Conversation, three independently-constructed aggregates related only by ConversationId, so
/// this needs a transaction (same multi-aggregate shape as SubmitPromotionHandler).
/// </summary>
public sealed class ClaimConversationHandler(
    IConversationReadService conversationReadService,
    IConversationWriteService conversationWriteService,
    IConversationQueueItemWriteService queueItemWriteService,
    IConversationAssignmentWriteService assignmentWriteService,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork) : ICommandHandler<ClaimConversationCommand>
{
    public async Task Handle(ClaimConversationCommand request, CancellationToken ct = default)
    {
        var conversation = await conversationReadService.GetByIdAsync(request.ConversationId, ct)
            ?? throw new NotFoundException(nameof(Conversation), request.ConversationId);

        var callerId = currentUser.GetUserId() ?? throw new UnauthorizedException();
        if (currentUser.GetRoles().Contains(ChatRoleConstants.Guest))
            throw new ForbiddenException("Guests cannot claim conversations.");

        var assignment = ConversationAssignment.Create(conversation.Id, callerId);

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await queueItemWriteService.MarkAssignedAsync(conversation.Id, ct);
            await assignmentWriteService.CreateAsync(assignment, ct);

            if (conversation.Status == ConversationStatus.Queued)
                await conversationWriteService.OpenAsync(conversation.Id, ct);
        }, ct: ct);
    }
}
