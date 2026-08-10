using NovaCore.BuildingBlock.Application.Abstractions.Common;

namespace NovaCore.Notification.Application.Features.NotificationRules.Queries.ListNotificationRules;

public sealed record ListNotificationRulesQuery(
    string? EventType,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedResult<NotificationRuleSummaryResponse>>;

public sealed record NotificationRuleSummaryResponse(
    Guid Id,
    string Name,
    string EventType,
    NotificationRuleStatus Status,
    int TargetCount,
    DateTime CreatedAt);
