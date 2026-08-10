namespace NovaCore.Notification.Application.Abstractions.Services;

/// <summary>Looks up the registered <see cref="IChannelSender"/> for a channel type.</summary>
public interface IChannelSenderResolver
{
    /// <exception cref="NotImplementedException">No sender is registered for <paramref name="channelType"/> yet.</exception>
    IChannelSender Resolve(NotificationChannelType channelType);
}
