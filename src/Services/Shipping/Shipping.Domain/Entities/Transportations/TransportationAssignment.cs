namespace NovaCore.Shipping.Domain.Entities.Transportations;

/// <summary>
/// Who and what is carrying one Transportation. Strict 1:1 extension of Transportation, so its
/// primary key *is* TransportationId (no surrogate id) per the shared-PK convention. PersonId /
/// VehicleId reference the standalone TransportationPerson / TransportationVehicle aggregates -
/// at least one of the two must be present.
/// </summary>
public sealed class TransportationAssignment : BaseEntity, IAuditable
{
    /// <summary>The primary key - shared with Transportation, not a surrogate.</summary>
    public Guid TransportationId { get; private set; }
    public Guid? PersonId { get; private set; }
    public Guid? VehicleId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public Guid? AssignedById { get; private set; }
    public string? Note { get; private set; }

    private TransportationAssignment() { }

    internal static TransportationAssignment Create(
        Guid transportationId,
        Guid? personId,
        Guid? vehicleId,
        Guid? assignedById = null,
        string? note = null)
    {
        if (personId is null && vehicleId is null)
            throw ExceptionFactory.RequiredField("An assignment needs at least a person or a vehicle.");

        return new TransportationAssignment
        {
            TransportationId = transportationId,
            PersonId = personId,
            VehicleId = vehicleId,
            AssignedAt = DateTime.UtcNow,
            AssignedById = assignedById,
            Note = note?.Trim(),
        };
    }
}
