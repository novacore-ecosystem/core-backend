namespace NovaCore.Notification.Domain.Entities;

/// <summary>
/// The element type NotificationRule.Create's bulk factory accepts (see Product.Create /
/// docs/conventions/domain-coding-conventions.md#2's Rule 1 exception) - not persisted itself,
/// NotificationRule.Create turns each model into an owned NotificationRuleTarget.
/// </summary>
public sealed record RuleTargetCreateModel(NotificationChannelType Channel, Guid TemplateId, NotificationPriority Priority);

/// <summary>
/// One action a <see cref="NotificationRule"/> performs when its event fires - e.g. "OrderCreated
/// -> create a User Notification AND an Email dispatch AND a Telegram dispatch" is three targets
/// on one rule. Channel may be InApp (produces a <see cref="UserNotification"/> directly) or any
/// real delivery channel (produces a <see cref="NotificationDispatch"/>) - which one happens is an
/// Application-layer concern when the rule actually executes, not something this entity decides.
/// Operations can disable a single target without touching business code (Enabled).
/// </summary>
public sealed class NotificationRuleTarget : BaseEntity<Guid>, ITenantEntity
{
    public Guid RuleId { get; private set; }
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

    private NotificationRuleTarget() { }

    internal static NotificationRuleTarget Create(Guid id, Guid ruleId, NotificationChannelType channel, Guid templateId, NotificationPriority priority)
    {
        if (templateId == Guid.Empty)
            throw ExceptionFactory.RequiredField("A rule target requires a template.");

        return new NotificationRuleTarget
        {
            Id = id,
            RuleId = ruleId,
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
