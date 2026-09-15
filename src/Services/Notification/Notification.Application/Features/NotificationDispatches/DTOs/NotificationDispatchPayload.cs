namespace NovaCore.Notification.Application.Features.NotificationDispatches.DTOs;

/// <summary>
/// Shape of <see cref="NotificationDispatch.Payload"/>'s JSON. Produced by whatever maps an
/// incoming trigger (an integration event, a direct API call, ...) into a dispatch, consumed by
/// whichever <see cref="NovaCore.Notification.Application.Abstractions.Services.IChannelSender"/> ends up
/// delivering it - keeps the two sides decoupled from any one channel's wire format.
/// </summary>
public sealed record NotificationDispatchPayload(
    Guid RecipientUserId,
    string Category,
    string Type,
    string Title,
    string Content,
    /// <summary>Required when the dispatch targets <see cref="Domain.Enums.NotificationChannelType.Email"/> - the producer is responsible for resolving it, this service has no user/email lookup of its own.</summary>
    string? RecipientEmail = null);
