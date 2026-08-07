namespace NovaCore.Shipping.Domain.Entities.Pickups;

/// <summary>
/// A scheduled collection of a Shipment's goods from a warehouse, merchant or customer.
/// Standalone aggregate referencing ShipmentId: a pickup can be rescheduled or fail on its own
/// (nobody home, warehouse closed) without changing the Shipment's status, and one Shipment may
/// need several pickup attempts.
/// </summary>
public sealed class Pickup : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid ShipmentId { get; private set; }
    public PickupType PickupType { get; private set; }
    public ShippingAddress Address { get; private set; } = default!;
    public string ContactName { get; private set; } = string.Empty;
    public PhoneNumber ContactPhone { get; private set; } = default!;
    public PickupStatus Status { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public DateTime? PickedUpAt { get; private set; }
    public string? FailureReason { get; private set; }
    public string? Note { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private Pickup() { }

    public static Pickup Create(
        Guid shipmentId,
        PickupType pickupType,
        ShippingAddress address,
        string contactName,
        PhoneNumber contactPhone,
        DateTime scheduledAt,
        string? note = null)
    {
        if (shipmentId == Guid.Empty)
            throw ExceptionFactory.RequiredField("Shipment id is required.");

        if (string.IsNullOrWhiteSpace(contactName))
            throw ExceptionFactory.RequiredField("Pickup contact name cannot be empty.");

        return new Pickup
        {
            Id = Guid.CreateVersion7(),
            ShipmentId = shipmentId,
            PickupType = pickupType,
            Address = address,
            ContactName = contactName.Trim(),
            ContactPhone = contactPhone,
            Status = PickupStatus.Scheduled,
            ScheduledAt = scheduledAt,
            Note = note?.Trim(),
        };
    }

    public void Start()
    {
        if (Status != PickupStatus.Scheduled)
            throw ExceptionFactory.InvalidStatus($"Cannot start a pickup in {Status} status.");

        Status = PickupStatus.InProgress;
    }

    public void Complete()
    {
        if (Status is not (PickupStatus.Scheduled or PickupStatus.InProgress))
            throw ExceptionFactory.InvalidStatus($"Cannot complete a pickup in {Status} status.");

        Status = PickupStatus.Completed;
        PickedUpAt = DateTime.UtcNow;
    }

    public void Fail(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw ExceptionFactory.RequiredField("A failure reason is required.");

        if (Status is PickupStatus.Completed or PickupStatus.Cancelled)
            throw ExceptionFactory.InvalidStatus($"Cannot fail a pickup in {Status} status.");

        Status = PickupStatus.Failed;
        FailureReason = reason.Trim();
    }

    public void Reschedule(DateTime scheduledAt)
    {
        if (Status is PickupStatus.Completed or PickupStatus.Cancelled)
            throw ExceptionFactory.InvalidStatus($"Cannot reschedule a pickup in {Status} status.");

        ScheduledAt = scheduledAt;
        Status = PickupStatus.Scheduled;
        FailureReason = null;
    }

    public void Cancel(string reason)
    {
        if (Status == PickupStatus.Completed)
            throw ExceptionFactory.InvalidStatus("Cannot cancel a completed pickup.");

        Status = PickupStatus.Cancelled;
        FailureReason = reason?.Trim();
    }
}
