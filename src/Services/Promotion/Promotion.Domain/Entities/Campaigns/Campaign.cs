namespace NovaCore.Promotion.Domain.Entities.Campaigns;

/// <summary>
/// Aggregate root for a marketing Campaign - the umbrella a Promotion may optionally belong to
/// (Promotion.CampaignId). CampaignApproval is related by CampaignId only (no navigation
/// collection here) - see docs/promotion-service/aggregates/campaign.md. CampaignBudget is
/// independently constructible but referenced from Campaign via BudgetId + the Budget navigation
/// below (Phase 2.6 correction - Budget resolves at the Persistence layer via FK, same as
/// Coupon.Campaign/Coupon.Promotion).
/// </summary>
public sealed class Campaign : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public EntityCode Code { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public CampaignStatus Status { get; private set; }
    public CampaignType Type { get; private set; }
    public int Priority { get; private set; }
    public Guid? BudgetId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public string TimeZone { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsEnabled { get; private set; }

    public CampaignBudget? Budget { get; private set; }
    public ICollection<CampaignSchedule> Schedules { get; private set; } = [];
    public ICollection<CampaignAudience> Audiences { get; private set; } = [];
    public ICollection<CampaignChannel> Channels { get; private set; } = [];
    public ICollection<CampaignTag> Tags { get; private set; } = [];
    public ICollection<CampaignAttachment> Attachments { get; private set; } = [];
    public ICollection<CampaignTranslation> Translations { get; private set; } = [];

    /// <summary>Inverse navigation only - Promotion is its own aggregate root and owns its own lifecycle; Campaign never constructs or removes a Promotion.</summary>
    public ICollection<PromotionEntity> Promotions { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private Campaign() { }

    public static Campaign Create(
        EntityCode code,
        string name,
        CampaignType type,
        DateTime startTime,
        DateTime endTime,
        string timeZone,
        string? description = null,
        int priority = 0,
        int displayOrder = 0)
    {
        ValidateName(name);
        ValidatePeriod(startTime, endTime, timeZone);

        return new Campaign
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            Description = description,
            Type = type,
            Status = CampaignStatus.Draft,
            Priority = priority,
            StartTime = startTime,
            EndTime = endTime,
            TimeZone = timeZone,
            DisplayOrder = displayOrder,
            IsEnabled = true,
        };
    }
    #endregion

    #region Details & lifecycle
    public void UpdateDetails(string name, string? description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public void Reschedule(DateTime startTime, DateTime endTime, string timeZone)
    {
        ValidatePeriod(startTime, endTime, timeZone);

        StartTime = startTime;
        EndTime = endTime;
        TimeZone = timeZone;
    }

    public void ChangePriority(int priority)
    {
        Priority = priority;
    }

    public void ChangeDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    /// <summary>Structural reference assignment only - CampaignBudget's own lifecycle lives outside this aggregate's navigation graph (see class remarks).</summary>
    public void AssignBudget(Guid? budgetId)
    {
        BudgetId = budgetId;
    }

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Campaign name cannot be empty.");
    }

    private static void ValidatePeriod(DateTime startTime, DateTime endTime, string timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
            throw ExceptionFactory.RequiredField("Time zone cannot be empty.");

        if (endTime <= startTime)
            throw ExceptionFactory.InvalidRange("End time must be after start time.");
    }
    #endregion

    #region Status
    public void Activate()
    {
        if (Status is not (CampaignStatus.Draft or CampaignStatus.Scheduled or CampaignStatus.Paused))
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
        if (Status is not (CampaignStatus.Active or CampaignStatus.Paused))
            throw ExceptionFactory.InvalidStatus($"Cannot complete a campaign in {Status} status.");

        Status = CampaignStatus.Completed;
    }

    public void Cancel()
    {
        if (Status is CampaignStatus.Completed or CampaignStatus.Cancelled)
            throw ExceptionFactory.InvalidStatus($"Cannot cancel a campaign in {Status} status.");

        Status = CampaignStatus.Cancelled;
    }
    #endregion

    #region Schedule
    public void AddSchedule(Period period, string? label = null)
    {
        Schedules.Add(CampaignSchedule.Create(Id, period, label, Schedules.Count));
    }

    public void RemoveSchedule(Guid scheduleId)
    {
        var schedule = Schedules.FirstOrDefault(s => s.Id == scheduleId)
            ?? throw ExceptionFactory.EntityNotFound<CampaignSchedule>(scheduleId);

        Schedules.Remove(schedule);
    }
    #endregion

    #region Audience
    public void AddAudience(string name, string? description = null)
    {
        Audiences.Add(CampaignAudience.Create(Id, name, description, Audiences.Count));
    }

    public void RemoveAudience(Guid audienceId)
    {
        var audience = Audiences.FirstOrDefault(a => a.Id == audienceId)
            ?? throw ExceptionFactory.EntityNotFound<CampaignAudience>(audienceId);

        Audiences.Remove(audience);
    }
    #endregion

    #region Channel
    public void AddChannel(string channel)
    {
        if (Channels.Any(c => c.Channel == channel))
            throw ExceptionFactory.Duplicate("This channel is already assigned to the campaign.");

        Channels.Add(CampaignChannel.Create(Id, channel));
    }

    public void RemoveChannel(Guid channelId)
    {
        var channel = Channels.FirstOrDefault(c => c.Id == channelId)
            ?? throw ExceptionFactory.EntityNotFound<CampaignChannel>(channelId);

        Channels.Remove(channel);
    }
    #endregion

    #region Tag
    public void AddTag(string label)
    {
        if (Tags.Any(t => t.Label == label))
            throw ExceptionFactory.Duplicate("This tag is already assigned to the campaign.");

        Tags.Add(CampaignTag.Create(Id, label));
    }

    public void RemoveTag(Guid tagId)
    {
        var tag = Tags.FirstOrDefault(t => t.Id == tagId)
            ?? throw ExceptionFactory.EntityNotFound<CampaignTag>(tagId);

        Tags.Remove(tag);
    }
    #endregion

    #region Attachment
    public void AddAttachment(string fileName, string url, string? contentType = null, long? sizeBytes = null)
    {
        Attachments.Add(CampaignAttachment.Create(Id, fileName, url, contentType, sizeBytes, Attachments.Count));
    }

    public void RemoveAttachment(Guid attachmentId)
    {
        var attachment = Attachments.FirstOrDefault(a => a.Id == attachmentId)
            ?? throw ExceptionFactory.EntityNotFound<CampaignAttachment>(attachmentId);

        Attachments.Remove(attachment);
    }
    #endregion

    #region Translations
    public void Translate(LanguageCode languageCode, string name, string? description)
    {
        ValidateName(name);

        var existing = Translations.FirstOrDefault(l => l.LanguageCode == languageCode);
        if (existing is not null)
        {
            existing.UpdateDetails(name, description);
            return;
        }

        Translations.Add(CampaignTranslation.Create(Id, languageCode, name, description));
    }
    #endregion
}
