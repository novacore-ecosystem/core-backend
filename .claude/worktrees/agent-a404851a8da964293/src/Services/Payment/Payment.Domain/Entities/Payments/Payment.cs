namespace NovaCore.Payment.Domain.Entities.Payments;

/// <summary>
/// Aggregate root of the payment lifecycle - the single source of truth for a monetary charge
/// against ReferenceType/ReferenceId. Never depends on a business module's assemblies; every
/// integration is purely through ReferenceType + ReferenceId.
/// </summary>
public sealed class Payment : AggregateRoot<Guid>, IAuditable, ITenantEntity, IIdempotentEntity
{
    public Guid? PaymentIntentId { get; private set; }
    public ReferenceType ReferenceType { get; private set; }
    public Guid ReferenceId { get; private set; }
    public Money Amount { get; private set; } = default!;
    public PaymentStatus Status { get; private set; }
    public Guid GatewayId { get; private set; }
    public Guid? PaymentMethodId { get; private set; }
    public string? IdempotencyKey { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public string? FailureCode { get; private set; }
    public string? FailureReason { get; private set; }
    public string? Metadata { get; private set; }

    public ICollection<PaymentItem> Items { get; private set; } = [];
    public ICollection<PaymentAttempt> Attempts { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private Payment() { }

    public static Payment Create(
        ReferenceType referenceType,
        Guid referenceId,
        Money amount,
        Guid gatewayId,
        Guid? paymentIntentId = null,
        Guid? paymentMethodId = null,
        string? idempotencyKey = null,
        DateTime? expiresAt = null,
        string? metadata = null)
    {
        return new Payment
        {
            Id = Guid.CreateVersion7(),
            PaymentIntentId = paymentIntentId,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Amount = amount,
            Status = PaymentStatus.Pending,
            GatewayId = gatewayId,
            PaymentMethodId = paymentMethodId,
            IdempotencyKey = idempotencyKey,
            ExpiresAt = expiresAt,
            Metadata = metadata,
        };
    }
    #endregion

    #region PaymentItem
    /// <summary>Only Payment may construct a PaymentItem (see PaymentItem.Create being internal).</summary>
    public void AddItem(PaymentItemType itemType, string description, Money amount, int quantity = 1)
    {
        Items.Add(PaymentItem.Create(Id, itemType, description, amount, quantity));
    }
    #endregion

    #region PaymentAttempt
    /// <summary>Only Payment may construct a PaymentAttempt (see PaymentAttempt.Create being internal).</summary>
    public PaymentAttempt StartAttempt(DateTime? expiresAt = null)
    {
        var attempt = PaymentAttempt.Create(Id, Attempts.Count + 1, expiresAt);
        Attempts.Add(attempt);
        return attempt;
    }
    #endregion
}
