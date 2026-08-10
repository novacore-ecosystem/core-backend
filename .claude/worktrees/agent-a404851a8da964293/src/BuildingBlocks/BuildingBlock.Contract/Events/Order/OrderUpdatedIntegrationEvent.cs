namespace NovaCore.BuildingBlock.Contract.Events.Order;

/// <summary>
/// Fired whenever a Pending order's item list is replaced via the manual UpdateOrder command.
/// CustomerId is carried explicitly, same rationale as OrderCancelledIntegrationEvent, so
/// Notification Service can target the recipient without a callback to Order Service.
/// </summary>
public sealed record OrderUpdatedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(OrderUpdatedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
