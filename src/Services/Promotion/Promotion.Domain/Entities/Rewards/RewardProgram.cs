namespace NovaCore.Promotion.Domain.Entities.Rewards;

/// <summary>
/// Aggregate root for a RewardProgram - owns Definitions/Distributions. RewardClaim/RewardExecution/
/// RewardReservation/RewardHistory are related by id only, not navigated from here - see
/// docs/promotion-service/aggregates/reward.md. Code now uses the shared EntityCode Value Object,
/// same as every other aggregate root (Phase 2.6 correction - closes the last gap the Phase 2.5
/// standardization review had flagged but not fixed).
/// </summary>
public sealed class RewardProgram : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public EntityCode Code { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProgramStatus Status { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }

    public ICollection<RewardDefinition> Definitions { get; private set; } = [];
    public ICollection<RewardDistribution> Distributions { get; private set; } = [];
    public ICollection<RewardProgramTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private RewardProgram() { }

    public static RewardProgram Create(
        EntityCode code,
        string name,
        DateTime startTime,
        DateTime endTime,
        string? description = null)
    {
        ValidateName(name);
        ValidatePeriod(startTime, endTime);

        return new RewardProgram
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            Description = description,
            Status = ProgramStatus.Draft,
            StartTime = startTime,
            EndTime = endTime,
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

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Reward program name cannot be empty.");
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
            throw ExceptionFactory.InvalidStatus($"Cannot activate a reward program in {Status} status.");

        Status = ProgramStatus.Active;
    }

    public void Pause()
    {
        if (Status != ProgramStatus.Active)
            throw ExceptionFactory.InvalidStatus($"Cannot pause a reward program in {Status} status.");

        Status = ProgramStatus.Paused;
    }

    public void Expire()
    {
        if (Status is not (ProgramStatus.Active or ProgramStatus.Paused))
            throw ExceptionFactory.InvalidStatus($"Cannot expire a reward program in {Status} status.");

        Status = ProgramStatus.Expired;
    }

    public void Archive()
    {
        if (Status != ProgramStatus.Expired)
            throw ExceptionFactory.InvalidStatus($"Cannot archive a reward program in {Status} status.");

        Status = ProgramStatus.Archived;
    }
    #endregion

    #region Definition
    public void AddDefinition(RewardType rewardType, string? configuration = null)
    {
        Definitions.Add(RewardDefinition.Create(Id, rewardType, configuration));
    }

    public void RemoveDefinition(Guid definitionId)
    {
        var definition = Definitions.FirstOrDefault(d => d.Id == definitionId)
            ?? throw ExceptionFactory.EntityNotFound<RewardDefinition>(definitionId);

        Definitions.Remove(definition);
    }
    #endregion

    #region Distribution
    public void AddDistribution(Guid? distributionJobId = null, DateTime? scheduledAt = null)
    {
        Distributions.Add(RewardDistribution.Create(Id, distributionJobId, scheduledAt));
    }

    public void RemoveDistribution(Guid distributionId)
    {
        var distribution = Distributions.FirstOrDefault(d => d.Id == distributionId)
            ?? throw ExceptionFactory.EntityNotFound<RewardDistribution>(distributionId);

        Distributions.Remove(distribution);
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

        Translations.Add(RewardProgramTranslation.Create(Id, languageCode, name, description));
    }
    #endregion
}
