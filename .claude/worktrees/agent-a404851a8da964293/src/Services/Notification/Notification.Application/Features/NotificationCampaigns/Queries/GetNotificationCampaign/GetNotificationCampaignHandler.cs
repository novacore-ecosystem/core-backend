using NovaCore.Notification.Application.Abstractions.Persistence.NotificationCampaigns;

using NovaCore.BuildingBlock.Application.Exceptions;

namespace NovaCore.Notification.Application.Features.NotificationCampaigns.Queries.GetNotificationCampaign;

public sealed class GetNotificationCampaignHandler(INotificationCampaignReadService notificationCampaignReadService)
    : IQueryHandler<GetNotificationCampaignQuery, GetNotificationCampaignResponse>
{
    public async Task<GetNotificationCampaignResponse> Handle(GetNotificationCampaignQuery request, CancellationToken ct = default)
    {
        var entity = await notificationCampaignReadService.GetByIdAsync(request.CampaignId, ct)
            ?? throw new NotFoundException("NotificationCampaign", request.CampaignId);

        var targets = entity.Targets
            .Select(t => new NotificationCampaignTargetResponse(t.Id, t.Channel, t.TemplateId, t.Priority, t.Enabled))
            .ToList();

        return new GetNotificationCampaignResponse(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Status,
            entity.GroupId,
            entity.Schedule.ExecutionType,
            entity.Schedule.StartAt,
            entity.Schedule.EndAt,
            entity.Schedule.CronExpression,
            entity.LastExecutedAt,
            entity.NextExecutionAt,
            targets,
            entity.CreatedAt);
    }
}
