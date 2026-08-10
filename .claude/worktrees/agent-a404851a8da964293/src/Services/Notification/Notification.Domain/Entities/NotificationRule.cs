namespace NovaCore.Notification.Domain.Entities;

/// <summary>
/// Answers "when EventType occurs, what notification actions should be created?" - e.g.
/// OrderCreated -> [User Notification, Email, Telegram]. EventType is an open string (business
/// event names are defined by every producing service and evolve independently of this one),
/// same reasoning as Audit's Service/EntityType fields. Adding/removing/toggling a target is how
/// Operations enables or disables one notification channel for an event without a code change or
/// redeploy - the whole point of normalizing targets instead of hardcoding them in a consumer.
/// </summary>
public sealed class NotificationRule : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public NotificationRuleStatus Status { get; private set; }
    public ICollection<NotificationRuleTarget> Targets { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private NotificationRule() { }

    public static NotificationRule Create(
        Guid id,
        string name,
        string description,
        string eventType,
        IEnumerable<RuleTargetCreateModel> targets)
    {
        ValidateName(name);
        ValidateEventType(eventType);

        var models = targets.ToList();
        if (models.Count == 0)
            throw ExceptionFactory.EmptyCollection("A rule must define at least one target.");

        var rule = new NotificationRule
        {
            Id = id,
            Name = name,
            Description = description,
            EventType = eventType,
            Status = NotificationRuleStatus.Active,
        };

        foreach (var model in models)
        {
            rule.Targets.Add(NotificationRuleTarget.Create(
                Guid.CreateVersion7(), id, model.Channel, model.TemplateId, model.Priority));
        }

        return rule;
    }

    public NotificationRuleTarget AddTarget(NotificationChannelType channel, Guid templateId, NotificationPriority priority)
    {
        var target = NotificationRuleTarget.Create(Guid.CreateVersion7(), Id, channel, templateId, priority);
        Targets.Add(target);
        return target;
    }

    public void RemoveTarget(Guid targetId)
    {
        if (Targets.Count <= 1)
            throw ExceptionFactory.InvalidState("Cannot remove the last target of a rule.");

        var target = Targets.FirstOrDefault(t => t.Id == targetId)
            ?? throw ExceptionFactory.EntityNotFound<NotificationRuleTarget>(targetId);

        Targets.Remove(target);
    }

    public void EnableTarget(Guid targetId)
    {
        var target = Targets.FirstOrDefault(t => t.Id == targetId)
            ?? throw ExceptionFactory.EntityNotFound<NotificationRuleTarget>(targetId);

        target.Enable();
    }

    public void DisableTarget(Guid targetId)
    {
        var target = Targets.FirstOrDefault(t => t.Id == targetId)
            ?? throw ExceptionFactory.EntityNotFound<NotificationRuleTarget>(targetId);

        target.Disable();
    }

    public void UpdateDetails(string name, string description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public void Enable()
    {
        Status = NotificationRuleStatus.Active;
    }

    public void Disable()
    {
        Status = NotificationRuleStatus.Inactive;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    public static bool IsValidEventType(string? eventType) => !string.IsNullOrWhiteSpace(eventType);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Rule name cannot be empty.");
    }

    private static void ValidateEventType(string eventType)
    {
        if (!IsValidEventType(eventType))
            throw ExceptionFactory.RequiredField("Rule event type cannot be empty.");
    }
}
