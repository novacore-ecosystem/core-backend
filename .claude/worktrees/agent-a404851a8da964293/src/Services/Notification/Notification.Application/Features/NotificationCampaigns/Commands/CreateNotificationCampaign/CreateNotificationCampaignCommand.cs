namespace NovaCore.Notification.Application.Features.NotificationCampaigns.Commands.CreateNotificationCampaign;

public sealed record CreateNotificationCampaignCommand(
    string Name,
    string Description,
    Guid GroupId,
    CampaignExecutionType ExecutionType,
    DateTime StartAt,
    DateTime? EndAt,
    string? CronExpression,
    IReadOnlyCollection<NotificationCampaignTargetInput> Targets) : ICommand<CreateNotificationCampaignResponse>;

public sealed record NotificationCampaignTargetInput(NotificationChannelType Channel, Guid TemplateId, NotificationPriority Priority);

public sealed record CreateNotificationCampaignResponse(Guid Id);
