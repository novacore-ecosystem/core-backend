using Mapster;

using Microsoft.AspNetCore.Authorization;

using NovaCore.Chat.Application.Abstractions.Persistence.Messages;
using NovaCore.Chat.Application.Features.Messages.DTOs;
using NovaCore.Chat.Infrastructure.SignalR.Groups;

namespace NovaCore.Chat.Infrastructure.SignalR.Hubs;

/// <summary>
/// Realtime chat connection. [Authorize] accepts both a real authenticated user's Auth-issued JWT
/// and a guest's short-lived GuestTokenGenerator-issued JWT - both flow through the exact same
/// JwtBearer pipeline (see docs on GuestTokenGenerator), so UserId here is either a real User.Id
/// or a guest Contact.Id, and IsGuest (HubBase) tells the two apart.
///
/// Whether a given UserId is actually allowed to join a given conversation (participant/contact
/// membership check) is deliberately not enforced here yet - that authorization rule belongs to
/// the Application layer once a real "join conversation" use case exists; this phase only
/// establishes the connection/group mechanics.
/// </summary>
[Authorize]
public sealed class ChatHub(IMessageReadService messageReadService) : HubBase<IChatHubClient>
{
    public const string Path = "/hubs/chat";

    /// <summary>Bounds a single recovery response - a much larger gap than this means the client should fall back to a normal paged history load instead of one hub call.</summary>
    private const int MaxRecoveryBatch = 200;

    public async Task JoinConversation(Guid conversationId)
    {
        await AddGroupAsync(ConversationGroups.Conversation(conversationId));
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        await RemoveGroupAsync(ConversationGroups.Conversation(conversationId));
    }

    /// <summary>
    /// Gap recovery - the caller (any participant, including the original sender reconnecting
    /// after a disconnect) supplies the last sequence it knows about and gets back everything
    /// after it, up to MaxRecoveryBatch. Works uniformly whether the missed messages were sent
    /// through SignalR or REST, since both paths land in the same Messages table.
    /// </summary>
    public async Task<IReadOnlyList<ChatMessageDto>> RecoverMessages(Guid conversationId, long afterSequence)
    {
        var messages = await messageReadService.GetSinceSequenceAsync(conversationId, afterSequence, MaxRecoveryBatch);

        return [.. messages.Select(m => m.Adapt<ChatMessageDto>())];
    }

    /// <summary>No persisted state - just relayed live to whoever else is currently in the group.</summary>
    public async Task StartTyping(Guid conversationId)
    {
        await Clients.OthersInGroup(ConversationGroups.Conversation(conversationId)).UserTyping(conversationId, UserId);
    }

    public async Task StopTyping(Guid conversationId)
    {
        await Clients.OthersInGroup(ConversationGroups.Conversation(conversationId)).UserStoppedTyping(conversationId, UserId);
    }
}
