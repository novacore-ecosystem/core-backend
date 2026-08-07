namespace NovaCore.Promotion.Domain.Entities.Loyalty;

/// <summary>
/// Aggregate root for a LoyaltyProgram - owns PointRules/PointPolicies/Accounts. PointTransaction/
/// PointLedger/PointExpiration/PointAdjustment/PointHistory are navigated bidirectionally from
/// PointAccount (or PointTransaction, for PointLedger), per Phase 3.1's navigation-completeness
/// pass, but remain unowned (public construction, no Add{Child} on PointAccount) - not navigated
/// from LoyaltyProgram itself - see docs/promotion-service/aggregates/loyalty.md.
/// </summary>
public sealed class LoyaltyProgram : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public EntityCode Code { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProgramStatus Status { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsEnabled { get; private set; }

    public ICollection<PointRule> PointRules { get; private set; } = [];
    public ICollection<PointPolicy> PointPolicies { get; private set; } = [];
    public ICollection<PointAccount> Accounts { get; private set; } = [];
    public ICollection<LoyaltyProgramTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private LoyaltyProgram() { }

    public static LoyaltyProgram Create(
        EntityCode code,
        string name,
        DateTime startTime,
        DateTime endTime,
        string? description = null,
        bool isDefault = false)
    {
        ValidateName(name);
        ValidatePeriod(startTime, endTime);

        return new LoyaltyProgram
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            Description = description,
            Status = ProgramStatus.Draft,
            StartTime = startTime,
            EndTime = endTime,
            IsDefault = isDefault,
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

    public void Reschedule(DateTime startTime, DateTime endTime)
    {
        ValidatePeriod(startTime, endTime);

        StartTime = startTime;
        EndTime = endTime;
    }

    public void MarkDefault() => IsDefault = true;

    public void UnmarkDefault() => IsDefault = false;

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Loyalty program name cannot be empty.");
    }

    private static void ValidatePeriod(DateTime startTime, DateTime endTime)
    {
        if (endTime <= startTime)
            throw ExceptionFactory.InvalidRange("End time must be after start time.");
    }
    #endregion

    #region Status
    public void Activate()
    {
        if (Status is not (ProgramStatus.Draft or ProgramStatus.Paused))
            throw ExceptionFactory.InvalidStatus($"Cannot activate a loyalty program in {Status} status.");

        Status = ProgramStatus.Active;
    }

    public void Pause()
    {
        if (Status != ProgramStatus.Active)
            throw ExceptionFactory.InvalidStatus($"Cannot pause a loyalty program in {Status} status.");

        Status = ProgramStatus.Paused;
    }

    public void Expire()
    {
        if (Status is not (ProgramStatus.Active or ProgramStatus.Paused))
            throw ExceptionFactory.InvalidStatus($"Cannot expire a loyalty program in {Status} status.");

        Status = ProgramStatus.Expired;
    }

    public void Archive()
    {
        if (Status != ProgramStatus.Expired)
            throw ExceptionFactory.InvalidStatus($"Cannot archive a loyalty program in {Status} status.");

        Status = ProgramStatus.Archived;
    }
    #endregion

    #region PointRule
    public void AddPointRule(string ruleType, int priority = 0)
    {
        PointRules.Add(PointRule.Create(Id, ruleType, priority));
    }

    public void RemovePointRule(Guid ruleId)
    {
        var rule = PointRules.FirstOrDefault(r => r.Id == ruleId)
            ?? throw ExceptionFactory.EntityNotFound<PointRule>(ruleId);

        PointRules.Remove(rule);
    }
    #endregion

    #region PointPolicy
    public void AddPointPolicy(string policyType, string? configuration = null)
    {
        PointPolicies.Add(PointPolicy.Create(Id, policyType, configuration));
    }

    public void RemovePointPolicy(Guid policyId)
    {
        var policy = PointPolicies.FirstOrDefault(p => p.Id == policyId)
            ?? throw ExceptionFactory.EntityNotFound<PointPolicy>(policyId);

        PointPolicies.Remove(policy);
    }
    #endregion

    #region Account
    public void AddAccount(Guid userId)
    {
        if (Accounts.Any(a => a.UserId == userId))
            throw ExceptionFactory.Duplicate("This user already has a point account under this loyalty program.");

        Accounts.Add(PointAccount.Create(Id, userId));
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

        Translations.Add(LoyaltyProgramTranslation.Create(Id, languageCode, name, description));
    }
    #endregion
}
