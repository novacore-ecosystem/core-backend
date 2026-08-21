using NovaCore.Chat.Application.Features.Messages.DTOs;

namespace NovaCore.Chat.Infrastructure.SignalR.Hubs;

/// <summary>Client-side methods ChatHub can invoke - the minimal reference shape (one push), following the same pattern as Notification's IGlobalHubClient.</summary>
public interface IChatHubClient
{
    Task ReceiveMessage(ChatMessageDto message);

    Task ConversationClosed(Guid conversationId, DateTime closedAt);
}
