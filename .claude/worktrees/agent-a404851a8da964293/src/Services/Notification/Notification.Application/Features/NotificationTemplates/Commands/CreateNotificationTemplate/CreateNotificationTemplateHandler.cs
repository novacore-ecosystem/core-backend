using NovaCore.Notification.Application.Abstractions.Persistence.NotificationTemplates;

namespace NovaCore.Notification.Application.Features.NotificationTemplates.Commands.CreateNotificationTemplate;

public sealed class CreateNotificationTemplateHandler(
    INotificationTemplateWriteService notificationTemplateWriteService) : ICommandHandler<CreateNotificationTemplateCommand, CreateNotificationTemplateResponse>
{
    public async Task<CreateNotificationTemplateResponse> Handle(CreateNotificationTemplateCommand request, CancellationToken ct = default)
    {
        var content = TemplateContent.Create(request.Subject, request.Body, request.Variables);

        var entity = NotificationTemplate.Create(
            Guid.CreateVersion7(), request.Name, request.Channel, content);

        await notificationTemplateWriteService.CreateAsync(entity, ct);

        return new CreateNotificationTemplateResponse(entity.Id);
    }
}
