namespace NovaCore.Shipping.Domain.Entities.Transportations;

/// <summary>
/// Append-only physical progress ping for one Transportation (a scan, a GPS position, a courier
/// note). Immutable - a correction is a new row, never an edit. Coordinate is optional: a
/// provider webhook may report a status change with no location at all.
/// </summary>
public sealed class TransportationTracking : BaseEntity<long>, IAuditable
{
    public Guid TransportationId { get; private set; }
    public TransportationStatus Status { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public GeoCoordinate? Coordinate { get; private set; }
    public string Description { get; private set; } = string.Empty;

    private TransportationTracking() { }

    internal static TransportationTracking Record(
        Guid transportationId,
        TransportationStatus status,
        GeoCoordinate? coordinate,
        string description)
    {
        return new TransportationTracking
        {
            TransportationId = transportationId,
            Status = status,
            RecordedAt = DateTime.UtcNow,
            Coordinate = coordinate,
            Description = string.IsNullOrWhiteSpace(description) ? status.ToString() : description.Trim(),
        };
    }
}
