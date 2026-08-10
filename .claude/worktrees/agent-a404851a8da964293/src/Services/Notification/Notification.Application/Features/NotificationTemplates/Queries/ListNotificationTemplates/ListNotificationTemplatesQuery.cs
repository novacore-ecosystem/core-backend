using NovaCore.BuildingBlock.Application.Abstractions.Common;

namespace NovaCore.Notification.Application.Features.NotificationTemplates.Queries.ListNotificationTemplates;

public sealed record ListNotificationTemplatesQuery(
    NotificationChannelType? Channel,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedResult<NotificationTemplateSummaryResponse>>;

public sealed record NotificationTemplateSummaryResponse(
    Guid Id,
    string Name,
    NotificationChannelType Channel,
    NotificationTemplateStatus Status,
    DateTime CreatedAt);
