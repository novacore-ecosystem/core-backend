using NovaCore.Chat.Application.Features.Messages.DTOs;

namespace NovaCore.Chat.Application.Abstractions.Services;

/// <summary>
/// Port onto the realtime push side (ChatHub/SignalR today) - kept separate from persistence so
/// handlers don't take a direct dependency on SignalR. A push is always best-effort/instant: a
/// disconnected client simply misses it and catches up via a normal GetMessages page load on
/// reconnect - there is nothing to retry (same reasoning Notification's IRealtimeNotifier
/// documents). Implemented in Chat.Infrastructure.
/// </summary>
public interface IChatRealtimeNotifier
{
    /// <summary>Pushed to every connection currently joined to the conversation's group (see ChatHub.JoinConversation).</summary>
    Task PushMessageAsync(Guid conversationId, ChatMessageDto message, CancellationToken ct = default);

    /// <summary>
    /// Notifies every connection joined to the conversation's group that it was closed - the guest
    /// client can clear its locally-persisted conversation id on receipt; GetConversationStatus
    /// remains the fallback for a client that missed this (disconnected, reconnected later).
    /// </summary>
    Task PushConversationClosedAsync(Guid conversationId, DateTime closedAt, CancellationToken ct = default);
}
