namespace NovaCore.Notification.Application.Features.NotificationChannels.Commands.UpdateNotificationChannelConfiguration;

public sealed record UpdateNotificationChannelConfigurationCommand(Guid ChannelId, string ConfigJson) : ICommand;
