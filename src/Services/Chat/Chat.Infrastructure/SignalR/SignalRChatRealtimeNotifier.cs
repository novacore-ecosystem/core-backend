using NovaCore.Chat.Application.Features.Messages.DTOs;
using NovaCore.Chat.Infrastructure.SignalR.Facade;

namespace NovaCore.Chat.Infrastructure.SignalR;

public sealed class SignalRChatRealtimeNotifier(ConversationHubFacade hub) : IChatRealtimeNotifier
{
    public async Task PushMessageAsync(Guid conversationId, ChatMessageDto message, CancellationToken ct = default)
    {
        await hub.Conversation(conversationId).ReceiveMessage(message);
    }

    public async Task PushConversationClosedAsync(Guid conversationId, DateTime closedAt, CancellationToken ct = default)
    {
        await hub.Conversation(conversationId).ConversationClosed(conversationId, closedAt);
    }
}
