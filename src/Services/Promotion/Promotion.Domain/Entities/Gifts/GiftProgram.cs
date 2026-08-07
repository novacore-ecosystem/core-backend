namespace NovaCore.Promotion.Domain.Entities.Gifts;

/// <summary>
/// Aggregate root for a GiftProgram - owns Items. GiftInventory/GiftReservation/GiftClaim/
/// GiftUsage are related by id only, not navigated from here - see
/// docs/promotion-service/aggregates/gift.md. Code now uses the shared EntityCode Value Object,
/// same as every other aggregate root (Phase 2.6 correction).
/// </summary>
public sealed class GiftProgram : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public EntityCode Code { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProgramStatus Status { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }

    public ICollection<GiftItem> Items { get; private set; } = [];
    public ICollection<GiftProgramTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private GiftProgram() { }

    public static GiftProgram Create(
        EntityCode code,
        string name,
        DateTime startTime,
        DateTime endTime,
        string? description = null)
    {
        ValidateName(name);
        ValidatePeriod(startTime, endTime);

        return new GiftProgram
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
            throw ExceptionFactory.RequiredField("Gift program name cannot be empty.");
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
            throw ExceptionFactory.InvalidStatus($"Cannot activate a gift program in {Status} status.");

        Status = ProgramStatus.Active;
    }

    public void Pause()
    {
        if (Status != ProgramStatus.Active)
            throw ExceptionFactory.InvalidStatus($"Cannot pause a gift program in {Status} status.");

        Status = ProgramStatus.Paused;
    }

    public void Expire()
    {
        if (Status is not (ProgramStatus.Active or ProgramStatus.Paused))
            throw ExceptionFactory.InvalidStatus($"Cannot expire a gift program in {Status} status.");

        Status = ProgramStatus.Expired;
    }

    public void Archive()
    {
        if (Status != ProgramStatus.Expired)
            throw ExceptionFactory.InvalidStatus($"Cannot archive a gift program in {Status} status.");

        Status = ProgramStatus.Archived;
    }
    #endregion

    #region Item
    public void AddItem(Guid productId, Quantity quantity, Guid? variantId = null)
    {
        Items.Add(GiftItem.Create(Id, productId, quantity, variantId));
    }

    public void RemoveItem(Guid itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw ExceptionFactory.EntityNotFound<GiftItem>(itemId);

        Items.Remove(item);
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

        Translations.Add(GiftProgramTranslation.Create(Id, languageCode, name, description));
    }
    #endregion
}
