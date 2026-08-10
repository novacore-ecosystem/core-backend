namespace NovaCore.BuildingBlock.Contract.Events.Order;

/// <summary>
/// Fired whenever an order transitions to Cancelled - both the manual CancelOrder command and
/// CreateOrderSaga's out-of-stock compensation path publish this (same Outbox row shape either
/// way, Reason distinguishes them e.g. "OutOfStock" vs "CancelledByCustomer"). CustomerId is
/// carried explicitly (not re-derived by the consumer) so Notification Service can target the
/// recipient without a callback to Order Service.
/// </summary>
public sealed record OrderCancelledIntegrationEvent(
    Guid TenantId,
    Guid OrderId,
    Guid CustomerId,
    string Reason,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(OrderCancelledIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
