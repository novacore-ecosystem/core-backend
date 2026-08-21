using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Chat.Domain.Entities.Conversations;

namespace NovaCore.Chat.Application.Common;

/// <summary>
/// Shared ownership check for guest/user-facing conversation actions (status check, close, rating)
/// - no fine-grained ConversationPermission system exists yet (see Chat.Domain's Permissions
/// scaffolding, unwired), so this is the simplest viable rule for this phase: a guest may only act
/// on the conversation their Contact is mapped to, an internal user may act on any conversation
/// they're authenticated for.
/// </summary>
public static class ConversationAccessGuard
{
    public static void EnsureCallerCanAccess(Conversation conversation, Guid callerId, bool isGuest)
    {
        if (!isGuest)
            return;

        if (conversation.ContactMappings.Any(c => c.ContactId == callerId))
            return;

        throw new ForbiddenException("You do not have access to this conversation.");
    }
}
