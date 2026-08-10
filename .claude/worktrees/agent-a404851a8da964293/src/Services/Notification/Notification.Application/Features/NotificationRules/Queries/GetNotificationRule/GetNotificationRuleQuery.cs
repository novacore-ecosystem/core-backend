namespace NovaCore.Notification.Application.Features.NotificationRules.Queries.GetNotificationRule;

public sealed record GetNotificationRuleQuery(Guid RuleId) : IQuery<GetNotificationRuleResponse>;

public sealed record GetNotificationRuleResponse(
    Guid Id,
    string Name,
    string Description,
    string EventType,
    NotificationRuleStatus Status,
    IReadOnlyCollection<NotificationRuleTargetResponse> Targets,
    DateTime CreatedAt);

public sealed record NotificationRuleTargetResponse(
    Guid Id,
    NotificationChannelType Channel,
    Guid TemplateId,
    NotificationPriority Priority,
    bool Enabled);
