using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationAssignments;
using NovaCore.Chat.Application.Abstractions.Persistence.Conversations;
using NovaCore.Chat.Application.Common;

namespace NovaCore.Chat.Application.Features.Conversations.Queries.GetConversationStatus;

public sealed class GetConversationStatusHandler(
    IConversationReadService conversationReadService,
    IConversationAssignmentReadService assignmentReadService,
    ICurrentUserService currentUser) : IQueryHandler<GetConversationStatusQuery, GetConversationStatusResponse>
{
    public async Task<GetConversationStatusResponse> Handle(GetConversationStatusQuery request, CancellationToken ct = default)
    {
        var conversation = await conversationReadService.GetByIdAsync(request.ConversationId, ct)
            ?? throw new NotFoundException(nameof(Conversation), request.ConversationId);

        var callerId = currentUser.GetUserId() ?? throw new UnauthorizedException();
        var isGuest = currentUser.GetRoles().Contains(ChatRoleConstants.Guest);
        ConversationAccessGuard.EnsureCallerCanAccess(conversation, callerId, isGuest);

        var activeAssignment = await assignmentReadService.GetActiveByConversationIdAsync(conversation.Id, ct);

        return new GetConversationStatusResponse(
            conversation.Id,
            conversation.Status,
            conversation.Title,
            activeAssignment?.AssigneeUserId,
            conversation.LastActivityAt);
    }
}
