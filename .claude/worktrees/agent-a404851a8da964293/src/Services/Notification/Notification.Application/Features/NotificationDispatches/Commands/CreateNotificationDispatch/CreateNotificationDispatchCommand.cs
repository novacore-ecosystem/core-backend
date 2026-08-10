namespace NovaCore.Notification.Application.Features.NotificationDispatches.Commands.CreateNotificationDispatch;

public record CreateNotificationDispatchCommand(
    DispatchReference Reference,
    NotificationChannelType[] Types,
    string Payload,
    Guid? TemplateId = null
    ) : ICommand;