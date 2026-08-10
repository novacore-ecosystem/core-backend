namespace NovaCore.Notification.Application.Features.NotificationTemplates.Queries.GetNotificationTemplate;

public sealed record GetNotificationTemplateQuery(Guid TemplateId) : IQuery<GetNotificationTemplateResponse>;

public sealed record GetNotificationTemplateResponse(
    Guid Id,
    string Name,
    NotificationChannelType Channel,
    string? Subject,
    string Body,
    IReadOnlyCollection<string> Variables,
    NotificationTemplateStatus Status,
    DateTime CreatedAt);
