namespace NovaCore.Order.Domain.Entities.Orders;

/// <summary>
/// Cancellation is business logic, not just a status - this captures who cancelled, why, and what
/// rollback work it implies (refund/inventory/payment), instead of the single CancellationReason
/// string Order used to carry directly. 1:1 with Order, sharing its primary key (OrderId), same
/// pattern as OrderOwner/OrderPrice/OrderPayment - except optional, since it only exists once an
/// order is actually cancelled (see OrderCancellationConfig for the nullable-dependent mapping).
/// </summary>
public sealed class OrderCancellation : BaseEntity, IAuditable, ITenantEntity
{
    public Guid OrderId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public Guid? CancelledByUserId { get; private set; }
    public string? CancelledByName { get; private set; }
    public DateTime CancelledAt { get; private set; }
    public bool RefundRequired { get; private set; }
    public bool InventoryRollbackRequired { get; private set; }
    public bool PaymentRollbackRequired { get; private set; }
    public string? Comment { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private OrderCancellation() { }

    /// <summary>Only Order may construct its Cancellation - same reasoning as OrderOwner.Create being internal.</summary>
    internal static OrderCancellation Create(
        Guid orderId,
        string reason,
        Guid? cancelledByUserId,
        string? cancelledByName,
        bool refundRequired,
        bool inventoryRollbackRequired,
        bool paymentRollbackRequired,
        string? comment = null)
    {
        return new OrderCancellation
        {
            OrderId = orderId,
            Reason = reason,
            CancelledByUserId = cancelledByUserId,
            CancelledByName = cancelledByName,
            CancelledAt = DateTime.UtcNow,
            RefundRequired = refundRequired,
            InventoryRollbackRequired = inventoryRollbackRequired,
            PaymentRollbackRequired = paymentRollbackRequired,
            Comment = comment,
        };
    }
}
