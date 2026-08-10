namespace NovaCore.BuildingBlock.Contract.Events.Shipping;

/// <summary>
/// Published when a user's saved shipping profile is matched to a verified address - the point
/// at which it becomes trustworthy enough to auto-fill without re-checking.
/// </summary>
public sealed record ShippingProfileVerifiedIntegrationEvent(
    Guid TenantId,
    Guid ShippingProfileId,
    Guid UserId,
    Guid VerifiedShippingAddressId,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(ShippingProfileVerifiedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
