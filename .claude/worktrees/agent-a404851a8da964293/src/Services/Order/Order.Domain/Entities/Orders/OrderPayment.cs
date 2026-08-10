namespace NovaCore.Order.Domain.Entities.Orders;

/// <summary>
/// Reference + snapshot of this order's payment - not the payment's system of record. Payment
/// Service (src/Services/Payment) now owns the full payment lifecycle (gateway, payment method,
/// account, attempts, tokens, ...); this row only ever holds a reference (PaymentId) plus the
/// minimal snapshot Order's own workflow needs (PaymentStatus/PaidAmount/PaidAt), updated via
/// RecordPayment (same "wholesale snapshot sync" shape as OrderShipping.UpdateSnapshot). 1:1 with
/// Order, sharing its primary key (OrderId) - see OrderPaymentConfig, same pattern as OrderOwner/
/// OrderPrice. See docs/services/payment-service.md and docs/reference/payment-ownership-boundaries.md.
/// </summary>
public sealed class OrderPayment : BaseEntity, IAuditable, ITenantEntity
{
    public Guid OrderId { get; private set; }

    /// <summary>Id of the payment record in Payment Service - null until Payment Service reports one.</summary>
    public Guid? PaymentId { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;
    public Money PaidAmount { get; private set; } = default!;
    public string CurrencyCode { get; private set; } = "USD";
    public DateTime? PaidAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private OrderPayment() { }

    /// <summary>Only Order may construct its Payment - same reasoning as OrderOwner.Create being internal.</summary>
    internal static OrderPayment Create(Guid orderId, string currencyCode = "USD")
    {
        return new OrderPayment
        {
            OrderId = orderId,
            PaymentStatus = PaymentStatus.Pending,
            PaidAmount = Money.Create(0),
            CurrencyCode = currencyCode,
        };
    }

    #region Snapshot sync
    /// <summary>
    /// Replaces the whole snapshot at once, mirroring OrderShipping.UpdateSnapshot - Order
    /// Service does not own payment state transitions, it only records Payment Service's latest
    /// report. Not yet called from anywhere (no consumer wired to Payment Service's integration
    /// events yet - see docs/services/payment-service.md, Phase 7).
    /// </summary>
    internal void RecordPayment(
        Guid? paymentId,
        PaymentStatus paymentStatus,
        Money paidAmount,
        DateTime? paidAt)
    {
        PaymentId = paymentId;
        PaymentStatus = paymentStatus;
        PaidAmount = paidAmount;
        PaidAt = paidAt;
    }
    #endregion
}
