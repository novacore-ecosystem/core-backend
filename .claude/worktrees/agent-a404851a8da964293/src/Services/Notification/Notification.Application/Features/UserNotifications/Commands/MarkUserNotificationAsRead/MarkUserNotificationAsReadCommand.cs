namespace NovaCore.Notification.Application.Features.UserNotifications.Commands.MarkUserNotificationAsRead;

public sealed record MarkUserNotificationAsReadCommand(Guid NotificationId) : ICommand;
