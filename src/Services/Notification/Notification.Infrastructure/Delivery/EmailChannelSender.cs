using System.Text.Json;

using NovaCore.BuildingBlock.Infrastructure.Mail.Abstractions;
using NovaCore.BuildingBlock.Infrastructure.Mail.Models;

using NovaCore.Notification.Application.Abstractions.Services;
using NovaCore.Notification.Application.Features.NotificationDispatches.DTOs;
using NovaCore.Notification.Domain.Entities;
using NovaCore.Notification.Domain.Enums;

namespace NovaCore.Notification.Infrastructure.Delivery;

/// <summary>
/// Sends a dispatch through Resend via the <see cref="IEmailSender"/> abstraction - Notification
/// Service is the only place in NovaCore that talks to Resend, everything else asks this service
/// to deliver a notification instead. See <see cref="ChannelSenderResolver"/> for the other channels.
/// </summary>
public sealed class EmailChannelSender(IEmailSender emailSender) : IChannelSender
{
    public NotificationChannelType ChannelType => NotificationChannelType.Email;

    public async Task SendAsync(NotificationDispatch dispatch, CancellationToken ct = default)
    {
        var payload = JsonSerializer.Deserialize<NotificationDispatchPayload>(dispatch.Payload)
            ?? throw new InvalidOperationException(
                $"Dispatch {dispatch.Id}'s payload is not a valid NotificationDispatchPayload.");

        if (string.IsNullOrWhiteSpace(payload.RecipientEmail))
            throw new InvalidOperationException(
                $"Dispatch {dispatch.Id} targets the Email channel but its payload has no RecipientEmail.");

        var message = new EmailMessage
        {
            Subject = payload.Title,
            HtmlBody = payload.Content,
            To = [new EmailAddress(payload.RecipientEmail)],
        };

        var result = await emailSender.SendAsync(message, ct);
        if (!result.IsSuccess)
            throw new InvalidOperationException($"Failed to send email for dispatch {dispatch.Id}: {result.Error}");
    }
}
