namespace NovaCore.BuildingBlock.Contract.Events.Shipping;

/// <summary>
/// Published when goods came back instead of reaching the recipient (refused, undeliverable).
/// The source module decides what that means commercially - refund, restock, retry.
/// </summary>
public sealed record TransportationReturnedIntegrationEvent(
    Guid TenantId,
    Guid TransportationId,
    string TransportationNumber,
    Guid ShipmentId,
    Guid ProviderId,
    string Reason,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(TransportationReturnedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
