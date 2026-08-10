namespace NovaCore.Notification.Application.Features.NotificationGroups.Queries.GetNotificationGroup;

public sealed record GetNotificationGroupQuery(Guid GroupId) : IQuery<GetNotificationGroupResponse>;

public sealed record GetNotificationGroupResponse(
    Guid Id,
    string Name,
    string Description,
    NotificationGroupStatus Status,
    AudienceType AudienceType,
    string? AudienceConfigJson,
    DateTime CreatedAt);
