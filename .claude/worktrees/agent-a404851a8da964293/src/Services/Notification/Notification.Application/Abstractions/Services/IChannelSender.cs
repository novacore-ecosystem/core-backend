namespace NovaCore.Notification.Application.Abstractions.Services;

/// <summary>
/// Delivers one <see cref="NotificationDispatch"/> through its <see cref="NotificationChannelType"/>.
/// One implementation per channel, resolved via <see cref="IChannelSenderResolver"/>. Throws on
/// delivery failure - the dispatch worker catches it and drives the retry/dead-letter state
/// machine (<see cref="NotificationDispatch.MarkFailed"/>), so implementations should not swallow
/// errors themselves.
/// </summary>
public interface IChannelSender
{
    NotificationChannelType ChannelType { get; }

    Task SendAsync(NotificationDispatch dispatch, CancellationToken ct = default);
}
