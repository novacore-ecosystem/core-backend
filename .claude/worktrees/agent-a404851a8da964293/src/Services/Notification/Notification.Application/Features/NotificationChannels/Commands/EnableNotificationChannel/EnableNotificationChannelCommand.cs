namespace NovaCore.Notification.Application.Features.NotificationChannels.Commands.EnableNotificationChannel;

public sealed record EnableNotificationChannelCommand(Guid ChannelId) : ICommand;
