using NovaCore.BuildingBlock.Application.Abstractions.Common;

namespace NovaCore.Notification.Application.Features.NotificationCampaigns.Queries.ListNotificationCampaigns;

public sealed record ListNotificationCampaignsQuery(
    CampaignStatus? Status,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedResult<NotificationCampaignSummaryResponse>>;

public sealed record NotificationCampaignSummaryResponse(
    Guid Id,
    string Name,
    CampaignStatus Status,
    CampaignExecutionType ExecutionType,
    DateTime? NextExecutionAt,
    DateTime CreatedAt);
