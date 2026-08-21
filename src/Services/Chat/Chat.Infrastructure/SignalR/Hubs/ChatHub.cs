using Microsoft.AspNetCore.Authorization;

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
public sealed class ChatHub : HubBase<IChatHubClient>
{
    public const string Path = "/hubs/chat";

    public async Task JoinConversation(Guid conversationId)
    {
        await AddGroupAsync(ConversationGroups.Conversation(conversationId));
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        await RemoveGroupAsync(ConversationGroups.Conversation(conversationId));
    }
}
