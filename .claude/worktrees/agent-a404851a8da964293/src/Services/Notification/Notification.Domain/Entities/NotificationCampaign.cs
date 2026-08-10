namespace NovaCore.Notification.Domain.Entities;

/// <summary>
/// A broadcast notification, sent once or on a recurring schedule to a <see cref="NotificationGroup"/>
/// audience. Never deleted - retired campaigns move to <see cref="CampaignStatus.Disabled"/> and
/// stay for reporting, audit, and copy-configuration. Execution (deciding "it's time to run" and
/// turning Targets into actual UserNotification rows / NotificationDispatch rows) is an
/// Application-layer/worker concern - this aggregate only tracks what to send, to whom, on what
/// schedule, and records when it last ran via <see cref="RecordExecution"/>.
/// </summary>
public sealed class NotificationCampaign : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public CampaignStatus Status { get; private set; }
    public NotificationSchedule Schedule { get; private set; } = null!;
    public Guid GroupId { get; private set; }
    public DateTime? LastExecutedAt { get; private set; }
    public DateTime? NextExecutionAt { get; private set; }
    public ICollection<NotificationCampaignTarget> Targets { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private NotificationCampaign() { }

    public static NotificationCampaign Create(
        Guid id,
        string name,
        string description,
        Guid groupId,
        NotificationSchedule schedule,
        IEnumerable<CampaignTargetCreateModel> targets)
    {
        ValidateName(name);

        var models = targets.ToList();
        if (models.Count == 0)
            throw ExceptionFactory.EmptyCollection("A campaign must define at least one target.");

        var campaign = new NotificationCampaign
        {
            Id = id,
            Name = name,
            Description = description,
            GroupId = groupId,
            Schedule = schedule,
            Status = CampaignStatus.Draft,
            NextExecutionAt = schedule.StartAt,
        };

        foreach (var model in models)
        {
            campaign.Targets.Add(NotificationCampaignTarget.Create(
                Guid.CreateVersion7(), id, model.Channel, model.TemplateId, model.Priority));
        }

        return campaign;
    }

    public NotificationCampaignTarget AddTarget(NotificationChannelType channel, Guid templateId, NotificationPriority priority)
    {
        var target = NotificationCampaignTarget.Create(Guid.CreateVersion7(), Id, channel, templateId, priority);
        Targets.Add(target);
        return target;
    }

    public void RemoveTarget(Guid targetId)
    {
        if (Targets.Count <= 1)
            throw ExceptionFactory.InvalidState("Cannot remove the last target of a campaign.");

        var target = Targets.FirstOrDefault(t => t.Id == targetId)
            ?? throw ExceptionFactory.EntityNotFound<NotificationCampaignTarget>(targetId);

        Targets.Remove(target);
    }

    public void EnableTarget(Guid targetId)
    {
        var target = Targets.FirstOrDefault(t => t.Id == targetId)
            ?? throw ExceptionFactory.EntityNotFound<NotificationCampaignTarget>(targetId);

        target.Enable();
    }

    public void DisableTarget(Guid targetId)
    {
        var target = Targets.FirstOrDefault(t => t.Id == targetId)
            ?? throw ExceptionFactory.EntityNotFound<NotificationCampaignTarget>(targetId);

        target.Disable();
    }

    public void UpdateDetails(string name, string description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public void UpdateSchedule(NotificationSchedule schedule)
    {
        Schedule = schedule;
        NextExecutionAt = schedule.StartAt;
    }

    public void Activate()
    {
        if (Status != CampaignStatus.Draft && Status != CampaignStatus.Paused)
            throw ExceptionFactory.InvalidStatus($"Cannot activate a campaign in {Status} status.");

        Status = CampaignStatus.Active;
    }

    public void Pause()
    {
        if (Status != CampaignStatus.Active)
            throw ExceptionFactory.InvalidStatus($"Cannot pause a campaign in {Status} status.");

        Status = CampaignStatus.Paused;
    }

    public void Complete()
    {
        if (Status != CampaignStatus.Active)
            throw ExceptionFactory.InvalidStatus($"Cannot complete a campaign in {Status} status.");

        Status = CampaignStatus.Completed;
        NextExecutionAt = null;
    }

    public void Disable()
    {
        if (Status == CampaignStatus.Disabled)
            throw ExceptionFactory.InvalidStatus("Campaign is already disabled.");

        Status = CampaignStatus.Disabled;
        NextExecutionAt = null;
    }

    /// <summary>Records that an execution just happened. Whether the campaign should also move to Completed (e.g. a Once campaign after its single run) is decided by the caller via <see cref="Complete"/> - this method only records the fact.</summary>
    public void RecordExecution(DateTime executedAtUtc, DateTime? nextExecutionAtUtc)
    {
        LastExecutedAt = executedAtUtc;
        NextExecutionAt = nextExecutionAtUtc;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Campaign name cannot be empty.");
    }
}
