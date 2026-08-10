namespace NovaCore.BuildingBlock.Contract.Events.Order;

/// <summary>
/// Fired the moment a Pending order + its items are persisted (CreateOrderHandler), inside the
/// same transaction/SaveChanges as the aggregate write. Two independent consumers react to it:
/// the CreateOrderSaga consumer (Order Service itself - drives inventory deduction/confirmation)
/// and Notification Service (pushes an instant "new order" realtime ping to the admin queue).
/// </summary>
public sealed record OrderCreatedIntegrationEvent(
    Guid TenantId,
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyCollection<OrderCreatedItem> Items,
    decimal TotalAmount,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(OrderCreatedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>VariantId is what OrderItem.ProductId actually holds - see docs/services/order-service.md's naming-mismatch note.</summary>
public sealed record OrderCreatedItem(Guid ProductId, Guid VariationId, string Name, int Quantity, decimal UnitPrice);
