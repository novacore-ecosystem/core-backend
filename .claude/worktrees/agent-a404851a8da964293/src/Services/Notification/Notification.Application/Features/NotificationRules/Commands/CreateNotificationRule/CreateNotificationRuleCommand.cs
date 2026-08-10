namespace NovaCore.Notification.Application.Features.NotificationRules.Commands.CreateNotificationRule;

public sealed record CreateNotificationRuleCommand(
    string Name,
    string Description,
    string EventType,
    IReadOnlyCollection<NotificationRuleTargetInput> Targets) : ICommand<CreateNotificationRuleResponse>;

public sealed record NotificationRuleTargetInput(NotificationChannelType Channel, Guid TemplateId, NotificationPriority Priority);

public sealed record CreateNotificationRuleResponse(Guid Id);
