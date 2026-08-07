namespace NovaCore.BuildingBlock.Contract.Events.Shipping;

/// <summary>
/// Published when a delivery address is confirmed reachable, with its resolved coordinate.
/// UserService may use this to enrich the user's own address book.
/// </summary>
public sealed record VerifiedAddressCreatedIntegrationEvent(
    Guid TenantId,
    Guid VerifiedShippingAddressId,
    Guid UserId,
    string FormattedAddress,
    decimal? Latitude,
    decimal? Longitude,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(VerifiedAddressCreatedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
