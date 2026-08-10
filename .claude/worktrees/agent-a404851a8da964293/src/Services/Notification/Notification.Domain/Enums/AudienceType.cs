namespace NovaCore.Notification.Domain.Enums;

/// <summary>How a <see cref="Entities.NotificationGroup"/> resolves its target audience.</summary>
public enum AudienceType
{
    All = 1,
    Roles = 2,
    SpecificUsers = 3,
    Segment = 4,
}
