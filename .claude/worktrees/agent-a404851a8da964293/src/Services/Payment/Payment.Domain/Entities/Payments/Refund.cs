namespace NovaCore.Payment.Domain.Entities.Payments;

/// <summary>Aggregate root for a refund against a Payment. One Payment may have multiple Refunds (e.g. partial refunds).</summary>
public sealed class Refund : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid PaymentId { get; private set; }

    /// <summary>Denormalized from Payment at creation time so Refund can be queried independently of Payment.</summary>
    public ReferenceType ReferenceType { get; private set; }
    public Guid ReferenceId { get; private set; }
    public Money Amount { get; private set; } = default!;
    public string Reason { get; private set; } = string.Empty;
    public RefundStatus Status { get; private set; }

    public ICollection<RefundAttempt> Attempts { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private Refund() { }

    public static Refund Create(
        Guid paymentId,
        ReferenceType referenceType,
        Guid referenceId,
        Money amount,
        string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw ExceptionFactory.RequiredField("Refund reason cannot be empty.");

        return new Refund
        {
            Id = Guid.CreateVersion7(),
            PaymentId = paymentId,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Amount = amount,
            Reason = reason.Trim(),
            Status = RefundStatus.Requested,
        };
    }

    /// <summary>Only Refund may construct a RefundAttempt (see RefundAttempt.Create being internal).</summary>
    public RefundAttempt StartAttempt()
    {
        var attempt = RefundAttempt.Create(Id, Attempts.Count + 1);
        Attempts.Add(attempt);
        return attempt;
    }
}
