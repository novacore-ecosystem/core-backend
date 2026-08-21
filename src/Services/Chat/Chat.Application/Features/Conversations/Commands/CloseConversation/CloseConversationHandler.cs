using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Chat.Application.Abstractions.Persistence.Conversations;
using NovaCore.Chat.Application.Abstractions.Services;
using NovaCore.Chat.Application.Common;

namespace NovaCore.Chat.Application.Features.Conversations.Commands.CloseConversation;

public sealed class CloseConversationHandler(
    IConversationReadService conversationReadService,
    IConversationWriteService conversationWriteService,
    IChatRealtimeNotifier realtimeNotifier,
    ICurrentUserService currentUser) : ICommandHandler<CloseConversationCommand>
{
    public async Task Handle(CloseConversationCommand request, CancellationToken ct = default)
    {
        var conversation = await conversationReadService.GetByIdAsync(request.ConversationId, ct)
            ?? throw new NotFoundException(nameof(Conversation), request.ConversationId);

        var callerId = currentUser.GetUserId() ?? throw new UnauthorizedException();
        var isGuest = currentUser.GetRoles().Contains(ChatRoleConstants.Guest);
        ConversationAccessGuard.EnsureCallerCanAccess(conversation, callerId, isGuest);

        await conversationWriteService.CloseAsync(conversation.Id, ct);

        var closedAt = DateTime.UtcNow;
        await realtimeNotifier.PushConversationClosedAsync(conversation.Id, closedAt, ct);
    }
}
