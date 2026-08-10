namespace NovaCore.Notification.Domain.Entities;

/// <summary>Element type for NotificationCampaign.Create's bulk factory - see RuleTargetCreateModel, same reasoning, campaign-scoped.</summary>
public sealed record CampaignTargetCreateModel(NotificationChannelType Channel, Guid TemplateId, NotificationPriority Priority);

/// <summary>
/// One channel+template a <see cref="NotificationCampaign"/> broadcasts through when it executes -
/// structurally identical to <see cref="NotificationRuleTarget"/> (both answer "which channel,
/// which template, what priority, is it enabled"), kept as a separate type because a campaign's
/// targets and a rule's targets are owned by, and evolve with, different aggregates.
/// </summary>
public sealed class NotificationCampaignTarget : BaseEntity<Guid>, ITenantEntity
{
    public Guid CampaignId { get; private set; }
    public NotificationChannelType Channel { get; private set; }
    public Guid TemplateId { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public bool Enabled { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private NotificationCampaignTarget() { }

    internal static NotificationCampaignTarget Create(Guid id, Guid campaignId, NotificationChannelType channel, Guid templateId, NotificationPriority priority)
    {
        if (templateId == Guid.Empty)
            throw ExceptionFactory.RequiredField("A campaign target requires a template.");

        return new NotificationCampaignTarget
        {
            Id = id,
            CampaignId = campaignId,
            Channel = channel,
            TemplateId = templateId,
            Priority = priority,
            Enabled = true,
        };
    }

    internal void Enable()
    {
        Enabled = true;
    }

    internal void Disable()
    {
        Enabled = false;
    }
}
