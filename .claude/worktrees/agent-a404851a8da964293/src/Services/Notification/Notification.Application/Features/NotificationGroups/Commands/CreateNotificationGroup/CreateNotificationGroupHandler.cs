using NovaCore.Notification.Application.Abstractions.Persistence.NotificationGroups;

namespace NovaCore.Notification.Application.Features.NotificationGroups.Commands.CreateNotificationGroup;

public sealed class CreateNotificationGroupHandler(
    INotificationGroupWriteService notificationGroupWriteService) : ICommandHandler<CreateNotificationGroupCommand, CreateNotificationGroupResponse>
{
    public async Task<CreateNotificationGroupResponse> Handle(CreateNotificationGroupCommand request, CancellationToken ct = default)
    {
        var audience = AudienceSelector.Create(request.AudienceType, request.AudienceConfigJson);

        var entity = NotificationGroup.Create(
            Guid.CreateVersion7(), request.Name, request.Description, audience);

        await notificationGroupWriteService.CreateAsync(entity, ct);

        return new CreateNotificationGroupResponse(entity.Id);
    }
}
