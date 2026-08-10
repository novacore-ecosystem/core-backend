namespace NovaCore.Notification.Application.Features.NotificationChannels.Commands.DisableNotificationChannel;

public sealed record DisableNotificationChannelCommand(Guid ChannelId) : ICommand;
