namespace NovaCore.Order.Domain.Entities.Returns;

/// <summary>
/// Return is business logic, not an OrderStatus - a partial or full return runs its own workflow
/// independent of Order.Status, which is why this is its own aggregate root referencing OrderId
/// rather than a child of Order. CustomerId is a point-in-time snapshot (same convention every
/// other Order-related snapshot follows) so "my returns" queries don't need to join Order.
/// </summary>
public sealed class ReturnOrder : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    private static readonly Dictionary<ReturnStatus, ReturnStatus[]> AllowedTransitions = new()
    {
        [ReturnStatus.Requested] = [ReturnStatus.Approved, ReturnStatus.Rejected, ReturnStatus.Cancelled],
        [ReturnStatus.Approved] = [ReturnStatus.ShippingBack, ReturnStatus.Cancelled],
        [ReturnStatus.ShippingBack] = [ReturnStatus.Received],
        [ReturnStatus.Received] = [ReturnStatus.Inspecting],
        [ReturnStatus.Inspecting] = [ReturnStatus.Refunding, ReturnStatus.Rejected],
        [ReturnStatus.Refunding] = [ReturnStatus.Completed],
    };

    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public ReturnStatus Status { get; private set; }
    public ICollection<ReturnItem> Items { get; private set; } = [];
    public Money TotalRefundAmount { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private ReturnOrder() { }

    public static ReturnOrder Create(
        Guid orderId,
        Guid customerId,
        IReadOnlyList<CreateReturnItemData> itemsData)
    {
        if (itemsData.Count == 0)
            throw ExceptionFactory.EmptyCollection("A return must contain at least one item.");

        var returnOrder = new ReturnOrder
        {
            Id = Guid.CreateVersion7(),
            OrderId = orderId,
            CustomerId = customerId,
            Status = ReturnStatus.Requested,
        };

        returnOrder.Items = [.. itemsData.Select(data =>
            ReturnItem.Create(
                returnOrder.Id,
                data.OrderItemId,
                data.ReasonId,
                data.Quantity,
                data.RefundAmount,
                data.Comment))];

        returnOrder.TotalRefundAmount = Money.Create(returnOrder.Items.Sum(i => i.RefundAmount.Value));

        return returnOrder;
    }
    #endregion

    #region Status transitions
    /// <summary>
    /// Single transition entry point instead of one method per status (Approve/Reject/
    /// MarkShippingBack/...) - the state graph branches (Approved or Rejected from Requested;
    /// Cancelled from either Requested or Approved) enough that a data-driven allow-list is
    /// clearer than nine near-identical methods. ReturnStatusHistory is not recorded here -
    /// entities can't raise events in this codebase (see AggregateRoot's remarks), so the
    /// Application-layer caller is expected to append one, same as Order's status transitions.
    /// </summary>
    public void TransitionTo(ReturnStatus newStatus)
    {
        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            throw ExceptionFactory.InvalidStatus($"Cannot transition a return from {Status} to {newStatus}.");

        Status = newStatus;
    }
    #endregion
}

/// <summary>Construction-only data for ReturnOrder.Create, same reasoning as Order's CreateOrderData.</summary>
public sealed record CreateReturnItemData(
    long OrderItemId,
    Guid ReasonId,
    Quantity Quantity,
    Money RefundAmount,
    string? Comment = null);
