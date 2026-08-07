namespace NovaCore.Promotion.Domain.Entities.Vouchers;

/// <summary>
/// Aggregate root for a Voucher - a stored-value instrument, always tied to a Promotion. Unlike
/// Coupon, no Promotion/Campaign navigation object was requested here - only the FK ids
/// (PromotionId/CampaignId) are exposed. VoucherBatch/VoucherExpiration/VoucherFreeze are related
/// by VoucherId only (not a navigation collection here) - see
/// docs/promotion-service/aggregates/voucher.md.
/// </summary>
public sealed class Voucher : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid PromotionId { get; private set; }
    public Guid? CampaignId { get; private set; }
    public EntityCode Code { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public VoucherType VoucherType { get; private set; }
    public VoucherStatus Status { get; private set; }
    public Currency Currency { get; private set; } = default!;
    public Money Amount { get; private set; } = default!;
    public Money Balance { get; private set; } = default!;
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public string TimeZone { get; private set; } = string.Empty;
    public Guid? OwnerId { get; private set; }
    public Guid? WalletId { get; private set; }

    public VoucherWallet? Wallet { get; private set; }
    public ICollection<VoucherIssue> Issues { get; private set; } = [];
    public ICollection<VoucherReservation> Reservations { get; private set; } = [];
    public ICollection<VoucherRedemption> Redemptions { get; private set; } = [];
    public ICollection<VoucherTransfer> Transfers { get; private set; } = [];
    public ICollection<VoucherHistory> History { get; private set; } = [];
    public ICollection<VoucherTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private Voucher() { }

    public static Voucher Create(
        Guid promotionId,
        EntityCode code,
        string name,
        VoucherType voucherType,
        Money amount,
        Currency currency,
        DateTime startTime,
        DateTime endTime,
        string timeZone,
        string? description = null,
        Guid? campaignId = null)
    {
        ValidateName(name);
        ValidatePeriod(startTime, endTime, timeZone);

        return new Voucher
        {
            Id = Guid.CreateVersion7(),
            PromotionId = promotionId,
            CampaignId = campaignId,
            Code = code,
            Name = name,
            Description = description,
            VoucherType = voucherType,
            Status = VoucherStatus.Draft,
            Currency = currency,
            Amount = amount,
            Balance = amount,
            StartTime = startTime,
            EndTime = endTime,
            TimeZone = timeZone,
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

    public void AssignCampaign(Guid? campaignId)
    {
        CampaignId = campaignId;
    }

    public void AssignOwner(Guid? ownerId)
    {
        OwnerId = ownerId;
    }

    public void AssignWallet(Guid? walletId)
    {
        WalletId = walletId;
    }

    /// <summary>Structural balance setter only - no debit/credit calculation lives here.</summary>
    public void UpdateBalance(Money balance)
    {
        Balance = balance;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Voucher name cannot be empty.");
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
        if (Status != VoucherStatus.Draft)
            throw ExceptionFactory.InvalidStatus($"Cannot submit a voucher in {Status} status for approval.");

        Status = VoucherStatus.PendingApproval;
    }

    public void Approve()
    {
        if (Status != VoucherStatus.PendingApproval)
            throw ExceptionFactory.InvalidStatus($"Cannot approve a voucher in {Status} status.");

        Status = VoucherStatus.Approved;
    }

    public void MarkIssued()
    {
        if (Status != VoucherStatus.Approved)
            throw ExceptionFactory.InvalidStatus($"Cannot mark a voucher in {Status} status as issued.");

        Status = VoucherStatus.Issued;
    }

    public void Activate()
    {
        if (Status is not (VoucherStatus.Issued or VoucherStatus.Reserved))
            throw ExceptionFactory.InvalidStatus($"Cannot activate a voucher in {Status} status.");

        Status = VoucherStatus.Active;
    }

    public void MarkReserved()
    {
        if (Status != VoucherStatus.Active)
            throw ExceptionFactory.InvalidStatus($"Cannot reserve a voucher in {Status} status.");

        Status = VoucherStatus.Reserved;
    }

    public void MarkRedeemed()
    {
        if (Status is not (VoucherStatus.Active or VoucherStatus.Reserved))
            throw ExceptionFactory.InvalidStatus($"Cannot redeem a voucher in {Status} status.");

        Status = VoucherStatus.Redeemed;
    }

    public void Expire()
    {
        if (Status is VoucherStatus.Redeemed or VoucherStatus.Cancelled or VoucherStatus.Archived)
            throw ExceptionFactory.InvalidStatus($"Cannot expire a voucher in {Status} status.");

        Status = VoucherStatus.Expired;
    }

    public void Cancel()
    {
        if (Status is VoucherStatus.Redeemed or VoucherStatus.Cancelled or VoucherStatus.Archived)
            throw ExceptionFactory.InvalidStatus($"Cannot cancel a voucher in {Status} status.");

        Status = VoucherStatus.Cancelled;
    }

    public void Archive()
    {
        if (Status is not (VoucherStatus.Expired or VoucherStatus.Cancelled or VoucherStatus.Redeemed))
            throw ExceptionFactory.InvalidStatus($"Cannot archive a voucher in {Status} status.");

        Status = VoucherStatus.Archived;
    }
    #endregion

    #region Issue
    public void AddIssue(Guid userId, Guid? distributionId)
    {
        Issues.Add(VoucherIssue.Create(Id, userId, distributionId));
    }
    #endregion

    #region Reservation
    public void AddReservation(Guid userId, Money reservedAmount, Guid? orderId, DateTime? expiredAt = null)
    {
        Reservations.Add(VoucherReservation.Create(Id, orderId, userId, reservedAmount, expiredAt));
    }

    public void ReleaseReservation(Guid reservationId)
    {
        var reservation = Reservations.FirstOrDefault(r => r.Id == reservationId)
            ?? throw ExceptionFactory.EntityNotFound<VoucherReservation>(reservationId);

        Reservations.Remove(reservation);
    }
    #endregion

    #region Redemption
    public void AddRedemption(Money redeemedAmount, Guid? orderId)
    {
        Redemptions.Add(VoucherRedemption.Create(Id, orderId, redeemedAmount));
    }
    #endregion

    #region Transfer
    public void AddTransfer(Guid fromUserId, Guid toUserId, Money amount)
    {
        Transfers.Add(VoucherTransfer.Create(Id, fromUserId, toUserId, amount));
    }
    #endregion

    #region History
    public void RecordHistory(string action, Guid? operatorId)
    {
        History.Add(VoucherHistory.Create(Id, action, operatorId));
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

        Translations.Add(VoucherTranslation.Create(Id, languageCode, name, description));
    }
    #endregion
}
