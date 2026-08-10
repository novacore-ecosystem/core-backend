using NovaCore.Notification.Application.Abstractions.Persistence.NotificationRules;

namespace NovaCore.Notification.Application.Features.NotificationRules.Commands.CreateNotificationRule;

public sealed class CreateNotificationRuleHandler(
    INotificationRuleWriteService notificationRuleWriteService) : ICommandHandler<CreateNotificationRuleCommand, CreateNotificationRuleResponse>
{
    public async Task<CreateNotificationRuleResponse> Handle(CreateNotificationRuleCommand request, CancellationToken ct = default)
    {
        var targets = request.Targets.Select(t => new RuleTargetCreateModel(t.Channel, t.TemplateId, t.Priority));

        var entity = NotificationRule.Create(
            Guid.CreateVersion7(), request.Name, request.Description, request.EventType, targets);

        await notificationRuleWriteService.CreateAsync(entity, ct);

        return new CreateNotificationRuleResponse(entity.Id);
    }
}
