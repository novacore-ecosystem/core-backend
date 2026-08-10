using NovaCore.Notification.Application.Abstractions.Persistence.NotificationCampaigns;

namespace NovaCore.Notification.Application.Features.NotificationCampaigns.Commands.CreateNotificationCampaign;

public sealed class CreateNotificationCampaignHandler(
    INotificationCampaignWriteService notificationCampaignWriteService) : ICommandHandler<CreateNotificationCampaignCommand, CreateNotificationCampaignResponse>
{
    public async Task<CreateNotificationCampaignResponse> Handle(CreateNotificationCampaignCommand request, CancellationToken ct = default)
    {
        var schedule = NotificationSchedule.Create(request.ExecutionType, request.StartAt, request.EndAt, request.CronExpression);
        var targets = request.Targets.Select(t => new CampaignTargetCreateModel(t.Channel, t.TemplateId, t.Priority));

        var entity = NotificationCampaign.Create(
            Guid.CreateVersion7(), request.Name, request.Description, request.GroupId, schedule, targets);

        await notificationCampaignWriteService.CreateAsync(entity, ct);

        return new CreateNotificationCampaignResponse(entity.Id);
    }
}
