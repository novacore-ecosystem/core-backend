namespace NovaCore.Shipping.Domain.Entities.VerifiedAddresses;

/// <summary>
/// A delivery address that has actually been reached (or explicitly checked) at least once, with
/// its resolved geo coordinate. Standalone user-level aggregate: it is the accumulated
/// deliverability knowledge of the platform, so later shipments to the same place can skip
/// re-validation and providers get a coordinate instead of free text.
/// </summary>
public sealed class VerifiedShippingAddress : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid UserId { get; private set; }
    public ShippingAddress Address { get; private set; } = default!;
    public GeoCoordinate? Coordinate { get; private set; }
    public VerificationStatus Status { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public Guid? VerifiedById { get; private set; }
    public string? RejectionReason { get; private set; }

    /// <summary>How many shipments have actually been delivered to this address - the confidence signal behind "verified".</summary>
    public int SuccessfulDeliveryCount { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private VerifiedShippingAddress() { }

    public static VerifiedShippingAddress Create(Guid userId, ShippingAddress address, GeoCoordinate? coordinate = null)
    {
        if (userId == Guid.Empty)
            throw ExceptionFactory.RequiredField("User id is required.");

        return new VerifiedShippingAddress
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Address = address,
            Coordinate = coordinate,
            Status = VerificationStatus.Pending,
            SuccessfulDeliveryCount = 0,
        };
    }

    public void Verify(GeoCoordinate? coordinate = null, Guid? verifiedById = null)
    {
        if (Status == VerificationStatus.Verified)
            throw ExceptionFactory.InvalidStatus("Address is already verified.");

        Coordinate = coordinate ?? Coordinate;
        Status = VerificationStatus.Verified;
        VerifiedAt = DateTime.UtcNow;
        VerifiedById = verifiedById;
        RejectionReason = null;
    }

    public void Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw ExceptionFactory.RequiredField("A rejection reason is required.");

        Status = VerificationStatus.Rejected;
        RejectionReason = reason.Trim();
    }

    public void RecordSuccessfulDelivery() => SuccessfulDeliveryCount++;
}
