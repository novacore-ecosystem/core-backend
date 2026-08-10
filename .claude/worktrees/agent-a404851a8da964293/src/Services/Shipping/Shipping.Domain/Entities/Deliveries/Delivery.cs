namespace NovaCore.Shipping.Domain.Entities.Deliveries;

/// <summary>
/// The customer-facing delivery outcome of one Transportation. Kept separate from
/// TransportationProof on purpose: proof exists for every handover (including warehouse
/// transfers and supplier imports), whereas a Delivery only exists when a Transportation is
/// actually delivering to an end recipient - it carries recipient-specific concerns (attempt
/// count, refusal, COD collection) that would be meaningless on an internal transfer.
/// </summary>
public sealed class Delivery : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid TransportationId { get; private set; }
    public string ReceiverName { get; private set; } = string.Empty;
    public PhoneNumber ReceiverPhone { get; private set; } = default!;
    public ShippingAddress Address { get; private set; } = default!;
    public DeliveryStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string? FailureReason { get; private set; }

    /// <summary>Cash-on-delivery amount to collect - zero when the shipment was prepaid.</summary>
    public Money CodAmount { get; private set; } = default!;
    public bool CodCollected { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private Delivery() { }

    public static Delivery Create(
        Guid transportationId,
        string receiverName,
        PhoneNumber receiverPhone,
        ShippingAddress address,
        Money codAmount)
    {
        if (transportationId == Guid.Empty)
            throw ExceptionFactory.RequiredField("Transportation id is required.");

        if (string.IsNullOrWhiteSpace(receiverName))
            throw ExceptionFactory.RequiredField("Receiver name cannot be empty.");

        return new Delivery
        {
            Id = Guid.CreateVersion7(),
            TransportationId = transportationId,
            ReceiverName = receiverName.Trim(),
            ReceiverPhone = receiverPhone,
            Address = address,
            Status = DeliveryStatus.Pending,
            AttemptCount = 0,
            CodAmount = codAmount,
            CodCollected = false,
        };
    }

    public void StartAttempt()
    {
        if (Status is DeliveryStatus.Delivered or DeliveryStatus.Returned)
            throw ExceptionFactory.InvalidStatus($"Cannot start a delivery attempt in {Status} status.");

        AttemptCount++;
        Status = DeliveryStatus.OutForDelivery;
    }

    public void Complete(bool codCollected = false)
    {
        if (Status != DeliveryStatus.OutForDelivery)
            throw ExceptionFactory.InvalidStatus($"Cannot complete a delivery in {Status} status.");

        if (CodAmount.Value > 0 && !codCollected)
            throw ExceptionFactory.InvalidState("A cash-on-delivery shipment cannot be completed without collecting the COD amount.");

        Status = DeliveryStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        CodCollected = codCollected;
    }

    public void Fail(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw ExceptionFactory.RequiredField("A failure reason is required.");

        if (Status is DeliveryStatus.Delivered or DeliveryStatus.Returned)
            throw ExceptionFactory.InvalidStatus($"Cannot fail a delivery in {Status} status.");

        Status = DeliveryStatus.Failed;
        FailureReason = reason.Trim();
    }

    public void Refuse(string reason)
    {
        if (Status is DeliveryStatus.Delivered)
            throw ExceptionFactory.InvalidStatus("Cannot refuse an already-delivered delivery.");

        Status = DeliveryStatus.Refused;
        FailureReason = reason?.Trim();
    }

    public void MarkReturned()
    {
        if (Status is not (DeliveryStatus.Failed or DeliveryStatus.Refused))
            throw ExceptionFactory.InvalidStatus($"Cannot return a delivery in {Status} status.");

        Status = DeliveryStatus.Returned;
    }
}
