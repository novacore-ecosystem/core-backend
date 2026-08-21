using Microsoft.AspNetCore.SignalR;

using NovaCore.Chat.Infrastructure.SignalR.Groups;
using NovaCore.Chat.Infrastructure.SignalR.Hubs;

namespace NovaCore.Chat.Infrastructure.SignalR.Facade;

/// <summary>Thin wrapper over IHubContext&lt;ChatHub, IChatHubClient&gt; - mirrors Notification's ActorHubFacade, simplified to Chat's single group shape (one hub, one group kind).</summary>
public sealed class ConversationHubFacade(IHubContext<ChatHub, IChatHubClient> hub)
{
    public IChatHubClient Conversation(Guid conversationId) => hub.Clients.Group(ConversationGroups.Conversation(conversationId));
}
