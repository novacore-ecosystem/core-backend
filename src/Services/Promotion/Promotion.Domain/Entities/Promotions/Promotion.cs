namespace NovaCore.Promotion.Domain.Entities.Promotions;

/// <summary>
/// Aggregate root for a Promotion - optionally belongs to a Campaign (CampaignId).
/// PromotionRuleGroup/PromotionPriority/PromotionExclusion are independently constructible and
/// related by PromotionId only (no Promotion navigation collection here) - see
/// docs/promotion-service/aggregates/promotion.md. PromotionCondition is owned by PromotionRule,
/// not by Promotion directly (Phase 2.6 correction - see PromotionRule.AddCondition).
/// </summary>
public sealed class Promotion : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid? CampaignId { get; private set; }
    public EntityCode Code { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public PromotionStatus Status { get; private set; }
    public PromotionType Type { get; private set; }
    public int Priority { get; private set; }
    public int Version { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public Currency Currency { get; private set; } = default!;
    public string TimeZone { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }

    public ICollection<PromotionVersion> Versions { get; private set; } = [];
    public ICollection<PromotionRule> Rules { get; private set; } = [];
    public ICollection<PromotionTarget> Targets { get; private set; } = [];
    public ICollection<PromotionBenefit> Benefits { get; private set; } = [];
    public ICollection<PromotionConstraint> Constraints { get; private set; } = [];
    public ICollection<PromotionUsageLimit> UsageLimits { get; private set; } = [];
    public PromotionExecutionPolicy? ExecutionPolicy { get; private set; }
    public PromotionStackingPolicy? StackingPolicy { get; private set; }
    public PromotionMetadata? Metadata { get; private set; }
    public ICollection<PromotionTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private Promotion() { }

    public static Promotion Create(
        EntityCode code,
        string name,
        PromotionType type,
        DateTime startTime,
        DateTime endTime,
        Currency currency,
        string timeZone,
        Guid? campaignId = null,
        string? description = null,
        int priority = 0)
    {
        ValidateName(name);
        ValidatePeriod(startTime, endTime, timeZone);

        return new Promotion
        {
            Id = Guid.CreateVersion7(),
            CampaignId = campaignId,
            Code = code,
            Name = name,
            Description = description,
            Type = type,
            Status = PromotionStatus.Draft,
            Priority = priority,
            Version = 1,
            StartTime = startTime,
            EndTime = endTime,
            Currency = currency,
            TimeZone = timeZone,
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

    public void AssignCampaign(Guid? campaignId)
    {
        CampaignId = campaignId;
    }

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;

    /// <summary>Structural version bump only - snapshotting behavior lives in AddVersion, not here.</summary>
    public void IncrementVersion()
    {
        Version++;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Promotion name cannot be empty.");
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
        if (Status is not (PromotionStatus.Draft or PromotionStatus.Paused))
            throw ExceptionFactory.InvalidStatus($"Cannot activate a promotion in {Status} status.");

        Status = PromotionStatus.Active;
    }

    public void Pause()
    {
        if (Status != PromotionStatus.Active)
            throw ExceptionFactory.InvalidStatus($"Cannot pause a promotion in {Status} status.");

        Status = PromotionStatus.Paused;
    }

    public void Expire()
    {
        if (Status is not (PromotionStatus.Active or PromotionStatus.Paused))
            throw ExceptionFactory.InvalidStatus($"Cannot expire a promotion in {Status} status.");

        Status = PromotionStatus.Expired;
    }

    public void Cancel()
    {
        if (Status is PromotionStatus.Expired or PromotionStatus.Cancelled)
            throw ExceptionFactory.InvalidStatus($"Cannot cancel a promotion in {Status} status.");

        Status = PromotionStatus.Cancelled;
    }
    #endregion

    #region Version
    public void AddVersion(string? note = null)
    {
        Versions.Add(PromotionVersion.Create(Id, Version, note));
    }
    #endregion

    #region Rule
    public void AddRule(string name, Guid? ruleGroupId = null)
    {
        Rules.Add(PromotionRule.Create(Id, ruleGroupId, name, Rules.Count));
    }

    public void RemoveRule(Guid ruleId)
    {
        var rule = Rules.FirstOrDefault(r => r.Id == ruleId)
            ?? throw ExceptionFactory.EntityNotFound<PromotionRule>(ruleId);

        Rules.Remove(rule);
    }
    #endregion

    #region Target
    public void AddTarget(PromotionTargetType targetType, string targetKey)
    {
        Targets.Add(PromotionTarget.Create(Id, targetType, targetKey));
    }

    public void RemoveTarget(Guid targetId)
    {
        var target = Targets.FirstOrDefault(t => t.Id == targetId)
            ?? throw ExceptionFactory.EntityNotFound<PromotionTarget>(targetId);

        Targets.Remove(target);
    }
    #endregion

    #region Benefit
    public void AddBenefit(PromotionBenefitType benefitType, decimal value)
    {
        Benefits.Add(PromotionBenefit.Create(Id, benefitType, value, Benefits.Count));
    }

    public void RemoveBenefit(Guid benefitId)
    {
        var benefit = Benefits.FirstOrDefault(b => b.Id == benefitId)
            ?? throw ExceptionFactory.EntityNotFound<PromotionBenefit>(benefitId);

        Benefits.Remove(benefit);
    }
    #endregion

    #region Constraint
    public void AddConstraint(PromotionConstraintType constraintType, string value)
    {
        Constraints.Add(PromotionConstraint.Create(Id, constraintType, value));
    }

    public void RemoveConstraint(Guid constraintId)
    {
        var constraint = Constraints.FirstOrDefault(c => c.Id == constraintId)
            ?? throw ExceptionFactory.EntityNotFound<PromotionConstraint>(constraintId);

        Constraints.Remove(constraint);
    }
    #endregion

    #region UsageLimit
    public void AddUsageLimit(PromotionUsageScope scope, int maxUsage)
    {
        UsageLimits.Add(PromotionUsageLimit.Create(Id, scope, maxUsage));
    }

    public void RemoveUsageLimit(Guid usageLimitId)
    {
        var usageLimit = UsageLimits.FirstOrDefault(u => u.Id == usageLimitId)
            ?? throw ExceptionFactory.EntityNotFound<PromotionUsageLimit>(usageLimitId);

        UsageLimits.Remove(usageLimit);
    }
    #endregion

    #region ExecutionPolicy
    public void SetExecutionPolicy(PromotionExecutionMode mode, int? maxExecutionsPerOrder = null, string? note = null)
    {
        if (ExecutionPolicy is not null)
        {
            ExecutionPolicy.UpdateDetails(mode, maxExecutionsPerOrder, note);
            return;
        }

        ExecutionPolicy = PromotionExecutionPolicy.Create(Id, mode, maxExecutionsPerOrder, note);
    }
    #endregion

    #region StackingPolicy
    public void SetStackingPolicy(PromotionStackingMode mode, string? note = null)
    {
        if (StackingPolicy is not null)
        {
            StackingPolicy.UpdateDetails(mode, note);
            return;
        }

        StackingPolicy = PromotionStackingPolicy.Create(Id, mode, note);
    }
    #endregion

    #region Metadata
    public void SetMetadata(PromotionMetadata metadata)
    {
        Metadata = metadata;
    }
    #endregion

    #region Translations
    public void Translate(LanguageCode languageCode, string name, string? description)
    {
        ValidateName(name);

        var existing = Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            existing.UpdateDetails(name, description);
            return;
        }

        Translations.Add(PromotionTranslation.Create(Id, languageCode, name, description));
    }
    #endregion
}
