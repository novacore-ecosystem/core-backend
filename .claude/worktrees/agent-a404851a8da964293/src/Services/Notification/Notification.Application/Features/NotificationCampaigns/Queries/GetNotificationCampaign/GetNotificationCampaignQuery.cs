namespace NovaCore.Notification.Application.Features.NotificationCampaigns.Queries.GetNotificationCampaign;

public sealed record GetNotificationCampaignQuery(Guid CampaignId) : IQuery<GetNotificationCampaignResponse>;

public sealed record GetNotificationCampaignResponse(
    Guid Id,
    string Name,
    string Description,
    CampaignStatus Status,
    Guid GroupId,
    CampaignExecutionType ExecutionType,
    DateTime StartAt,
    DateTime? EndAt,
    string? CronExpression,
    DateTime? LastExecutedAt,
    DateTime? NextExecutionAt,
    IReadOnlyCollection<NotificationCampaignTargetResponse> Targets,
    DateTime CreatedAt);

public sealed record NotificationCampaignTargetResponse(
    Guid Id,
    NotificationChannelType Channel,
    Guid TemplateId,
    NotificationPriority Priority,
    bool Enabled);
