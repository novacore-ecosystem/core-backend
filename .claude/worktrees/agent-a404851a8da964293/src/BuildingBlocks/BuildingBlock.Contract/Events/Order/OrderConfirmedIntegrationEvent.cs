namespace NovaCore.BuildingBlock.Contract.Events.Order;

/// <summary>
/// Fired by CreateOrderSaga's ConfirmOrderStep once inventory has been successfully deducted and
/// the order's status has been moved Pending -> Confirmed, in the same transaction as that status
/// change. Notification Service reacts to this to persist a UserNotification and push a realtime
/// "order confirmed" update to the customer - see docs/reference/create-order-saga.md.
/// </summary>
public sealed record OrderConfirmedIntegrationEvent(
    Guid TenantId,
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(OrderConfirmedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
