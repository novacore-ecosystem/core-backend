namespace NovaCore.Notification.Domain.Enums;

/// <summary>
/// Disabled is terminal but never deleted - campaigns are kept for reporting, audit, and
/// copy-configuration even once retired (see <see cref="Entities.NotificationCampaign.Disable"/>).
/// </summary>
public enum CampaignStatus
{
    Draft = 1,
    Active = 2,
    Paused = 3,
    Completed = 4,
    Disabled = 5,
}
