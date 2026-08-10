namespace NovaCore.BuildingBlock.Contract.Events.Order;

/// <summary>
/// Fired whenever an order transitions Confirmed -> Completed via the manual CompleteOrder
/// command (admin/fulfillment action - no automated saga step reaches Completed today).
/// CustomerId is carried explicitly, same rationale as OrderCancelledIntegrationEvent, so
/// Notification Service can target the recipient without a callback to Order Service.
/// </summary>
public sealed record OrderCompletedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(OrderCompletedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
