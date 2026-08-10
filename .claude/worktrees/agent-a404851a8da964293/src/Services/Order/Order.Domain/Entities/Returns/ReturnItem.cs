namespace NovaCore.Order.Domain.Entities.Returns;

/// <summary>
/// One line of a ReturnOrder - a partial quantity of a single OrderItem. References OrderItemId
/// only (no navigation, same shadow-reference convention every other Order child uses); the
/// OrderItem itself never changes once the order is placed, so there's nothing to keep in sync.
/// </summary>
public sealed class ReturnItem : BaseEntity<long>, IAuditable, ITenantEntity
{
    public Guid ReturnOrderId { get; private set; }
    public long OrderItemId { get; private set; }
    public Guid ReasonId { get; private set; }
    public Quantity Quantity { get; private set; } = default!;
    public Money RefundAmount { get; private set; } = default!;
    public string? Comment { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ReturnItem() { }

    /// <summary>Only ReturnOrder may construct its ReturnItem rows - same reasoning as OrderItem.Create being internal.</summary>
    internal static ReturnItem Create(
        Guid returnOrderId,
        long orderItemId,
        Guid reasonId,
        Quantity quantity,
        Money refundAmount,
        string? comment = null)
    {
        return new ReturnItem
        {
            ReturnOrderId = returnOrderId,
            OrderItemId = orderItemId,
            ReasonId = reasonId,
            Quantity = quantity,
            RefundAmount = refundAmount,
            Comment = comment,
        };
    }
}
