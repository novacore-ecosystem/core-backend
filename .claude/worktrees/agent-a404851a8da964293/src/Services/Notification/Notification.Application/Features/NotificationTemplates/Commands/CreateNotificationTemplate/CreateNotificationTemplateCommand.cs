namespace NovaCore.Notification.Application.Features.NotificationTemplates.Commands.CreateNotificationTemplate;

public sealed record CreateNotificationTemplateCommand(
    string Name,
    NotificationChannelType Channel,
    string? Subject,
    string Body,
    IReadOnlyCollection<string>? Variables) : ICommand<CreateNotificationTemplateResponse>;

public sealed record CreateNotificationTemplateResponse(Guid Id);
