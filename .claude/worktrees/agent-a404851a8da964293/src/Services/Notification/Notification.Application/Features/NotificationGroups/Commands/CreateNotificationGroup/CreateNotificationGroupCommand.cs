namespace NovaCore.Notification.Application.Features.NotificationGroups.Commands.CreateNotificationGroup;

public sealed record CreateNotificationGroupCommand(
    string Name,
    string Description,
    AudienceType AudienceType,
    string? AudienceConfigJson) : ICommand<CreateNotificationGroupResponse>;

public sealed record CreateNotificationGroupResponse(Guid Id);
