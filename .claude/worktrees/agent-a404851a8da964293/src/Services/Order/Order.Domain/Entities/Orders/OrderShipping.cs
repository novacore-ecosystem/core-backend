namespace NovaCore.Order.Domain.Entities.Orders;

/// <summary>
/// Reference + snapshot of this order's shipment - not the shipment's system of record. Shipment
/// lifecycle (routing, driver, packages, tracking events, warehouse routing) belongs to a future
/// Shipping Service; this row only ever holds whatever that service last reported, via
/// UpdateSnapshot (same "wholesale snapshot sync" shape as ProductCatalog). Order Service does not
/// enforce a shipping state machine - Status is just the last known value. Split out from Order
/// itself so the core order/status/items data isn't coupled to shipping columns, same reasoning
/// as OrderOwner/OrderPrice.
/// </summary>
public sealed class OrderShipping : BaseEntity<Guid>, IAuditable, ITenantEntity, IIdempotentEntity
{
    public Guid OrderId { get; private set; }
    public string ReceiverName { get; private set; } = string.Empty;
    public PhoneNumber ReceiverPhone { get; private set; } = default!;
    public string Address { get; private set; } = string.Empty;
    public ShippingMethod ShippingMethod { get; private set; } = ShippingMethod.Standard;

    /// <summary>Id of the shipment record in the (future) Shipping Service - null until that service exists and reports one.</summary>
    public Guid? ShippingReferenceId { get; private set; }
    public string? Carrier { get; private set; }
    public string? TrackingNumber { get; private set; }
    public ShippingStatus Status { get; private set; } = ShippingStatus.Pending;
    public DateTime? EstimatedDelivery { get; private set; }
    public Money ShippingFee { get; private set; } = default!;
    public string Note { get; private set; } = string.Empty;
    public string? IdempotencyKey { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private OrderShipping() { }

    internal static OrderShipping Create(
        Guid orderId,
        string receiverName,
        PhoneNumber receiverPhone,
        string address,
        ShippingMethod shippingMethod,
        string note,
        string? idempotencyKey = null)
    {
        return new OrderShipping
        {
            Id = Guid.CreateVersion7(),
            OrderId = orderId,
            ReceiverName = receiverName,
            ReceiverPhone = receiverPhone,
            Address = address,
            ShippingMethod = shippingMethod,
            Note = note,
            IdempotencyKey = idempotencyKey,
            // No fee calculation happens in Order Service anymore - the Shipping Service will
            // report the real fee via UpdateSnapshot once it exists.
            ShippingFee = Money.Create(0),
        };
    }
    #endregion

    #region Shipping information
    public void UpdateContact(
        string receiverName,
        PhoneNumber receiverPhone,
        string address,
        string idempotencyKey)
    {
        if (Status is not ShippingStatus.Pending)
            throw ExceptionFactory.InvalidStatus($"Receiver ({Status}) cannot be modified while the Order is being delivering.");

        ReceiverName = receiverName;
        ReceiverPhone = receiverPhone;
        Address = address;
        IdempotencyKey = idempotencyKey;
    }

    public void UpdateNote(string note)
    {
        if (Status is ShippingStatus.Delivered or ShippingStatus.Canceled)
            throw ExceptionFactory.InvalidStatus($"Note cannot be edited once the Order is completed.");

        Note = note;
    }
    #endregion

    #region Snapshot sync
    /// <summary>
    /// Replaces the whole snapshot at once, mirroring how ProductCatalog gets refreshed from
    /// Product's integration events - there is no incremental Mark* transition here because Order
    /// Service does not own the shipment's state machine, it only records the Shipping Service's
    /// latest report. Not yet called from anywhere (no Shipping Service exists to call it from).
    /// </summary>
    internal void UpdateSnapshot(
        Guid? shippingReferenceId,
        string? carrier,
        string? trackingNumber,
        ShippingStatus status,
        DateTime? estimatedDelivery,
        Money shippingFee)
    {
        ShippingReferenceId = shippingReferenceId;
        Carrier = carrier;
        TrackingNumber = trackingNumber;
        Status = status;
        EstimatedDelivery = estimatedDelivery;
        ShippingFee = shippingFee;
    }

    public void Cancel()
    {
        if (Status == ShippingStatus.Delivered)
            throw ExceptionFactory.InvalidStatus($"Cannot cancel shipping when status is {Status}.");

        Status = ShippingStatus.Canceled;
    }
    #endregion
}
