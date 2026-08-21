using NovaCore.Chat.Application.Features.Messages.DTOs;

namespace NovaCore.Chat.Infrastructure.SignalR.Hubs;

/// <summary>Client-side methods ChatHub can invoke - the minimal reference shape (one push), following the same pattern as Notification's IGlobalHubClient.</summary>
public interface IChatHubClient
{
    Task ReceiveMessage(ChatMessageDto message);

    Task ConversationClosed(Guid conversationId, DateTime closedAt);

    /// <summary>Ephemeral, no persistence - a lost event (disconnect) is fine, currently-connected participants just see live typing activity.</summary>
    Task UserTyping(Guid conversationId, Guid userId);

    Task UserStoppedTyping(Guid conversationId, Guid userId);
}
