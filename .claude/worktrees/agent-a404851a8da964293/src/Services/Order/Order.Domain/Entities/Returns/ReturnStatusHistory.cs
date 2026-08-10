namespace NovaCore.Order.Domain.Entities.Returns;

/// <summary>
/// Append-only record of one transition on a ReturnOrder's lifecycle - same shape and reasoning
/// as NovaCore.Order.Domain.Entities.Orders.OrderStatusHistory.
/// </summary>
public sealed class ReturnStatusHistory : BaseEntity<long>, ITenantEntity
{
    public Guid ReturnOrderId { get; private set; }
    public ReturnStatus PreviousStatus { get; private set; }
    public ReturnStatus CurrentStatus { get; private set; }
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

    private ReturnStatusHistory() { }

    public static ReturnStatusHistory Record(
        Guid returnOrderId,
        ReturnStatus previousStatus,
        ReturnStatus currentStatus,
        Guid? changedByUserId,
        string? changedByName,
        string? reason = null,
        string? comment = null)
    {
        return new ReturnStatusHistory
        {
            ReturnOrderId = returnOrderId,
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
