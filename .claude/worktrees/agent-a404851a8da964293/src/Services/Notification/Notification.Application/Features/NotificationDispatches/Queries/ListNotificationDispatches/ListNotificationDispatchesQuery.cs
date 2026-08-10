using NovaCore.BuildingBlock.Application.Abstractions.Common;

namespace NovaCore.Notification.Application.Features.NotificationDispatches.Queries.ListNotificationDispatches;

public sealed record ListNotificationDispatchesQuery(
    DispatchStatus? Status,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedResult<NotificationDispatchSummaryResponse>>;

public sealed record NotificationDispatchSummaryResponse(
    Guid Id,
    string ReferenceType,
    string ReferenceId,
    NotificationChannelType Channel,
    DispatchStatus Status,
    int RetryCount,
    DateTime CreatedAt);
