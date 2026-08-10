using System.Text.Json;

using NovaCore.Notification.Application.Abstractions.Services;
using NovaCore.Notification.Application.Features.NotificationDispatches.DTOs;
using NovaCore.Notification.Application.Features.UserNotifications.DTOs;
using NovaCore.Notification.Domain.Entities;
using NovaCore.Notification.Domain.Enums;
using NovaCore.Notification.Infrastructure.SignalR.Facade;
using NovaCore.Notification.Infrastructure.SignalR.Hubs.Global;

namespace NovaCore.Notification.Infrastructure.Delivery;

/// <summary>
/// Pushes a dispatch to the recipient's live SignalR connection(s) via <see cref="GlobalHub"/>.
/// The only channel with a real delivery implementation so far - Email/Telegram/Facebook/Zalo/Push
/// have no provider wired up yet, see <see cref="ChannelSenderResolver"/>.
/// </summary>
public sealed class SignalRChannelSender(
    ActorHubFacade<GlobalHub, IGlobalHubClient, IGlobalHubClient> hub) : IChannelSender
{
    public NotificationChannelType ChannelType => NotificationChannelType.SignalR;

    public async Task SendAsync(NotificationDispatch dispatch, CancellationToken ct = default)
    {
        var payload = JsonSerializer.Deserialize<NotificationDispatchPayload>(dispatch.Payload)
            ?? throw new InvalidOperationException(
                $"Dispatch {dispatch.Id}'s payload is not a valid NotificationDispatchPayload.");

        var dto = new NotificationDto(
            payload.RecipientUserId,
            payload.Category,
            payload.Type,
            payload.Title,
            payload.Content,
            Metadata: "{}",
            NotificationPriority.Normal,
            NotificationStatus.Unread,
            ExpiredAt: DateTime.UtcNow.AddDays(30));

        await hub.Member(payload.RecipientUserId).ReceiveNotification(dto);
    }
}
