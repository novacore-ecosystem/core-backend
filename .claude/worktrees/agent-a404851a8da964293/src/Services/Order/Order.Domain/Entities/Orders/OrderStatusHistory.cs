namespace NovaCore.Order.Domain.Entities.Orders;

/// <summary>
/// Append-only record of one transition/event on an Order's lifecycle. Never modified after
/// creation - closest existing shape precedent is
/// NovaCore.BuildingBlock.Persistence.Ef.Inbox.InboxRetryHistory (private setters, single factory,
/// no mutators). Unlike Order's other children this is not IAuditable - it is already an audit
/// log, auditing it would just record itself. PreviousStatus/CurrentStatus stay nullable because
/// several EventType values (Paid/Packed/Shipped/Delivered/Returned/Refunded/Reopened) are not
/// Order.Status transitions at all - see OrderStatusHistoryType's remarks.
/// </summary>
public sealed class OrderStatusHistory : BaseEntity<long>, ITenantEntity
{
    public Guid OrderId { get; private set; }
    public OrderStatusHistoryType EventType { get; private set; }
    public OrderStatus? PreviousStatus { get; private set; }
    public OrderStatus? CurrentStatus { get; private set; }
    public Guid? ChangedByUserId { get; private set; }
    public string? ChangedByName { get; private set; }
    public string? Reason { get; private set; }
    public string? Comment { get; private set; }
    public DateTime ChangedAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private OrderStatusHistory() { }

    public static OrderStatusHistory Record(
        Guid orderId,
        OrderStatusHistoryType eventType,
        OrderStatus? previousStatus,
        OrderStatus? currentStatus,
        Guid? changedByUserId,
        string? changedByName,
        string? reason = null,
        string? comment = null)
    {
        return new OrderStatusHistory
        {
            OrderId = orderId,
            EventType = eventType,
            PreviousStatus = previousStatus,
            CurrentStatus = currentStatus,
            ChangedByUserId = changedByUserId,
            ChangedByName = changedByName,
            Reason = reason,
            Comment = comment,
            ChangedAt = DateTime.UtcNow,
        };
    }
}
