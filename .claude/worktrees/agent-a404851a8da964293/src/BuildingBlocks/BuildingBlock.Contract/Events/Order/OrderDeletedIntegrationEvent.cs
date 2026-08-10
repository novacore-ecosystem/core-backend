namespace NovaCore.BuildingBlock.Contract.Events.Order;

/// <summary>
/// Fired whenever an order is hard-deleted via the manual DeleteOrder command. Only Pending or
/// Cancelled orders can be deleted (see DeleteOrderHandler) - both states never deducted stock,
/// so no Inventory compensation is needed here, unlike OrderCancelledIntegrationEvent.
/// </summary>
public sealed record OrderDeletedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(OrderDeletedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
