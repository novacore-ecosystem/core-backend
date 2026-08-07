using NovaCore.Promotion.Domain.Entities.Campaigns;

namespace NovaCore.Promotion.Domain.Entities.Coupons;

/// <summary>
/// Aggregate root for a Coupon - always belongs to a Promotion, optionally to a Campaign and to a
/// CouponBatch. CouponCode (individual issued codes) and CouponApproval now also have Codes/Approvals
/// navigation collections here (Phase 3.1 reversed the earlier FK-only decision) - see
/// docs/promotion-service/aggregates/coupon.md. The former CouponCode entity/Value Object name
/// collision (Phase 2.2) no longer applies - the per-aggregate Code Value Object was consolidated
/// into the shared EntityCode during the Phase 2.5 Domain Standardization Review, so no alias is
/// needed here anymore.
/// </summary>
public sealed class Coupon : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid PromotionId { get; private set; }
    public Guid? CampaignId { get; private set; }
    public Guid? BatchId { get; private set; }
    public EntityCode Code { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public CouponStatus Status { get; private set; }
    public CouponVisibility Visibility { get; private set; }
    public CouponType CouponType { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public string TimeZone { get; private set; } = string.Empty;
    public int? MaxUsage { get; private set; }
    public int? MaxUsagePerUser { get; private set; }
    public int CurrentUsage { get; private set; }
    public bool IsEnabled { get; private set; }

    public PromotionEntity? Promotion { get; private set; }
    public Campaign? Campaign { get; private set; }
    public CouponBatch? Batch { get; private set; }
    public ICollection<CouponReservation> Reservations { get; private set; } = [];
    public ICollection<CouponUsage> Usages { get; private set; } = [];
    public ICollection<CouponHistory> History { get; private set; } = [];
    public ICollection<CouponVersion> Versions { get; private set; } = [];
    public ICollection<CouponCode> Codes { get; private set; } = [];
    public ICollection<CouponApproval> Approvals { get; private set; } = [];
    public ICollection<CouponTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private Coupon() { }

    public static Coupon Create(
        Guid promotionId,
        EntityCode code,
        string name,
        CouponType couponType,
        DateTime startTime,
        DateTime endTime,
        string timeZone,
        string? description = null,
        CouponVisibility visibility = CouponVisibility.Public,
        Guid? campaignId = null,
        Guid? batchId = null,
        int? maxUsage = null,
        int? maxUsagePerUser = null)
    {
        ValidateName(name);
        ValidatePeriod(startTime, endTime, timeZone);

        return new Coupon
        {
            Id = Guid.CreateVersion7(),
            PromotionId = promotionId,
            CampaignId = campaignId,
            BatchId = batchId,
            Code = code,
            Name = name,
            Description = description,
            Status = CouponStatus.Draft,
            Visibility = visibility,
            CouponType = couponType,
            StartTime = startTime,
            EndTime = endTime,
            TimeZone = timeZone,
            MaxUsage = maxUsage,
            MaxUsagePerUser = maxUsagePerUser,
            CurrentUsage = 0,
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

    public void ChangeVisibility(CouponVisibility visibility)
    {
        Visibility = visibility;
    }

    public void ChangeUsageLimits(int? maxUsage, int? maxUsagePerUser)
    {
        MaxUsage = maxUsage;
        MaxUsagePerUser = maxUsagePerUser;
    }

    public void AssignCampaign(Guid? campaignId)
    {
        CampaignId = campaignId;
    }

    public void AssignBatch(Guid? batchId)
    {
        BatchId = batchId;
    }

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Coupon name cannot be empty.");
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
    public void SubmitForApproval()
    {
        if (Status != CouponStatus.Draft)
            throw ExceptionFactory.InvalidStatus($"Cannot submit a coupon in {Status} status for approval.");

        Status = CouponStatus.PendingApproval;
    }

    public void Approve()
    {
        if (Status != CouponStatus.PendingApproval)
            throw ExceptionFactory.InvalidStatus($"Cannot approve a coupon in {Status} status.");

        Status = CouponStatus.Approved;
    }

    public void Schedule()
    {
        if (Status != CouponStatus.Approved)
            throw ExceptionFactory.InvalidStatus($"Cannot schedule a coupon in {Status} status.");

        Status = CouponStatus.Scheduled;
    }

    public void Activate()
    {
        if (Status is not (CouponStatus.Scheduled or CouponStatus.Approved or CouponStatus.Paused))
            throw ExceptionFactory.InvalidStatus($"Cannot activate a coupon in {Status} status.");

        Status = CouponStatus.Active;
    }

    public void Pause()
    {
        if (Status != CouponStatus.Active)
            throw ExceptionFactory.InvalidStatus($"Cannot pause a coupon in {Status} status.");

        Status = CouponStatus.Paused;
    }

    public void Expire()
    {
        if (Status is not (CouponStatus.Active or CouponStatus.Paused))
            throw ExceptionFactory.InvalidStatus($"Cannot expire a coupon in {Status} status.");

        Status = CouponStatus.Expired;
    }

    public void Cancel()
    {
        if (Status is CouponStatus.Expired or CouponStatus.Cancelled or CouponStatus.Archived)
            throw ExceptionFactory.InvalidStatus($"Cannot cancel a coupon in {Status} status.");

        Status = CouponStatus.Cancelled;
    }

    public void Archive()
    {
        if (Status is not (CouponStatus.Expired or CouponStatus.Cancelled))
            throw ExceptionFactory.InvalidStatus($"Cannot archive a coupon in {Status} status.");

        Status = CouponStatus.Archived;
    }
    #endregion

    #region Reservation
    public void AddReservation(Guid userId, string reservationToken, DateTime? expiredAt = null)
    {
        Reservations.Add(CouponReservation.Create(Id, userId, reservationToken, expiredAt));
    }

    public void ReleaseReservation(Guid reservationId)
    {
        var reservation = Reservations.FirstOrDefault(r => r.Id == reservationId)
            ?? throw ExceptionFactory.EntityNotFound<CouponReservation>(reservationId);

        Reservations.Remove(reservation);
    }
    #endregion

    #region Usage
    /// <summary>Structural bookkeeping only - CurrentUsage is a plain counter increment, no eligibility/limit enforcement lives here.</summary>
    public void RecordUsage(Guid userId, Guid? orderId)
    {
        Usages.Add(CouponUsage.Create(Id, userId, orderId));
        CurrentUsage++;
    }
    #endregion

    #region History
    public void RecordHistory(string action, Guid? operatorId)
    {
        History.Add(CouponHistory.Create(Id, action, operatorId));
    }
    #endregion

    #region Version
    public void AddVersion(int version, string? snapshot = null)
    {
        Versions.Add(CouponVersion.Create(Id, version, snapshot));
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

        Translations.Add(CouponTranslation.Create(Id, languageCode, name, description));
    }
    #endregion
}
