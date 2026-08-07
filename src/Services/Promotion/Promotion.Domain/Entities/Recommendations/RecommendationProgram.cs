namespace NovaCore.Promotion.Domain.Entities.Recommendations;

/// <summary>
/// Aggregate root for a RecommendationProgram - owns Rules/Products. RecommendationScore/
/// RecommendationHistory are related by id only, not navigated from here - see
/// docs/promotion-service/aggregates/recommendation.md. No TimeZone property, same as
/// LoyaltyProgram.
/// </summary>
public sealed class RecommendationProgram : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public EntityCode Code { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProgramStatus Status { get; private set; }
    public RecommendationType RecommendationType { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public int Priority { get; private set; }

    public ICollection<RecommendationRule> Rules { get; private set; } = [];
    public ICollection<RecommendationProduct> Products { get; private set; } = [];
    public ICollection<RecommendationHistory> History { get; private set; } = [];
    public ICollection<RecommendationProgramTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private RecommendationProgram() { }

    public static RecommendationProgram Create(
        EntityCode code,
        string name,
        RecommendationType recommendationType,
        DateTime startTime,
        DateTime endTime,
        string? description = null,
        int priority = 0)
    {
        ValidateName(name);
        ValidatePeriod(startTime, endTime);

        return new RecommendationProgram
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            Description = description,
            Status = ProgramStatus.Draft,
            RecommendationType = recommendationType,
            StartTime = startTime,
            EndTime = endTime,
            Priority = priority,
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

    public void ChangePriority(int priority)
    {
        Priority = priority;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Recommendation program name cannot be empty.");
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
            throw ExceptionFactory.InvalidStatus($"Cannot activate a recommendation program in {Status} status.");

        Status = ProgramStatus.Active;
    }

    public void Pause()
    {
        if (Status != ProgramStatus.Active)
            throw ExceptionFactory.InvalidStatus($"Cannot pause a recommendation program in {Status} status.");

        Status = ProgramStatus.Paused;
    }

    public void Expire()
    {
        if (Status is not (ProgramStatus.Active or ProgramStatus.Paused))
            throw ExceptionFactory.InvalidStatus($"Cannot expire a recommendation program in {Status} status.");

        Status = ProgramStatus.Expired;
    }

    public void Archive()
    {
        if (Status != ProgramStatus.Expired)
            throw ExceptionFactory.InvalidStatus($"Cannot archive a recommendation program in {Status} status.");

        Status = ProgramStatus.Archived;
    }
    #endregion

    #region Rule
    public void AddRule(string ruleType, string? configuration = null, int priority = 0)
    {
        Rules.Add(RecommendationRule.Create(Id, ruleType, configuration, priority));
    }

    public void RemoveRule(Guid ruleId)
    {
        var rule = Rules.FirstOrDefault(r => r.Id == ruleId)
            ?? throw ExceptionFactory.EntityNotFound<RecommendationRule>(ruleId);

        Rules.Remove(rule);
    }
    #endregion

    #region Product
    public void AddProduct(Guid productId, decimal score, int displayOrder = 0)
    {
        if (Products.Any(p => p.ProductId == productId))
            throw ExceptionFactory.Duplicate("This product is already part of the recommendation program.");

        Products.Add(RecommendationProduct.Create(Id, productId, score, displayOrder));
    }

    public void RemoveProduct(Guid recommendationProductId)
    {
        var product = Products.FirstOrDefault(p => p.Id == recommendationProductId)
            ?? throw ExceptionFactory.EntityNotFound<RecommendationProduct>(recommendationProductId);

        Products.Remove(product);
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

        Translations.Add(RecommendationProgramTranslation.Create(Id, languageCode, name, description));
    }
    #endregion
}
