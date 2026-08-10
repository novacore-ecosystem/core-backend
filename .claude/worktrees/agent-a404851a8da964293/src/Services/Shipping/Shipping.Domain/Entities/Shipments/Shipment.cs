namespace NovaCore.Shipping.Domain.Entities.Shipments;

/// <summary>
/// A logistics *intention*: "these goods must move from A to B". A Shipment is never bound to a
/// ShippingProvider - execution (and therefore the provider) lives on Transportation, so one
/// Shipment can spawn several sequential Transportation attempts (retry-on-failure) without the
/// Shipment itself being recreated or rewritten. The originating business context is referenced
/// only through SourceType + SourceReferenceId; ShippingService never references Order,
/// Inventory or any other module's types.
/// </summary>
public sealed class Shipment : AggregateRoot<Guid>, IAuditable, ITenantEntity, IIdempotentEntity
{
    public ShipmentNumber ShipmentNumber { get; private set; } = default!;
    public ShipmentType ShipmentType { get; private set; }
    public SourceType SourceType { get; private set; }

    /// <summary>Opaque id owned by the source module (an OrderId, a WarehouseTransferId, ...) - never a foreign key.</summary>
    public Guid SourceReferenceId { get; private set; }
    public ShipmentStatus Status { get; private set; }

    public ShippingAddress SenderAddress { get; private set; } = default!;
    public string SenderName { get; private set; } = string.Empty;
    public PhoneNumber SenderPhone { get; private set; } = default!;

    public ShippingAddress ReceiverAddress { get; private set; } = default!;
    public string ReceiverName { get; private set; } = string.Empty;
    public PhoneNumber ReceiverPhone { get; private set; } = default!;

    /// <summary>Value declared for insurance/COD purposes - zero when the source module declares nothing.</summary>
    public Money DeclaredValue { get; private set; } = default!;
    public DateTime? RequestedPickupAt { get; private set; }
    public DateTime? ExpectedDeliveryAt { get; private set; }
    public string? Note { get; private set; }
    public string? FailureReason { get; private set; }
    public string? CancellationReason { get; private set; }

    public ICollection<ShipmentItem> Items { get; private set; } = [];
    public ICollection<ShipmentEvent> Events { get; private set; } = [];
    public ICollection<Package> Packages { get; private set; } = [];

    public string? IdempotencyKey { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    // ============================================================================
    // Construction
    // Builds the Shipment in Draft with its identity, parties and source reference
    // resolved. Items/Packages are attached afterwards through the aggregate's own
    // methods, which are the only way a child may be constructed.
    // ============================================================================

    #region Construction
    private Shipment() { }

    public static Shipment Create(
        ShipmentType shipmentType,
        SourceType sourceType,
        Guid sourceReferenceId,
        string senderName,
        PhoneNumber senderPhone,
        ShippingAddress senderAddress,
        string receiverName,
        PhoneNumber receiverPhone,
        ShippingAddress receiverAddress,
        Money declaredValue,
        DateTime? requestedPickupAt = null,
        DateTime? expectedDeliveryAt = null,
        string? note = null,
        string? idempotencyKey = null)
    {
        if (string.IsNullOrWhiteSpace(senderName))
            throw ExceptionFactory.RequiredField("Sender name cannot be empty.");

        if (string.IsNullOrWhiteSpace(receiverName))
            throw ExceptionFactory.RequiredField("Receiver name cannot be empty.");

        if (sourceReferenceId == Guid.Empty)
            throw ExceptionFactory.RequiredField("Source reference id is required.");

        return new Shipment
        {
            Id = Guid.CreateVersion7(),
            ShipmentNumber = ShipmentNumber.Create(),
            ShipmentType = shipmentType,
            SourceType = sourceType,
            SourceReferenceId = sourceReferenceId,
            Status = ShipmentStatus.Draft,
            SenderName = senderName.Trim(),
            SenderPhone = senderPhone,
            SenderAddress = senderAddress,
            ReceiverName = receiverName.Trim(),
            ReceiverPhone = receiverPhone,
            ReceiverAddress = receiverAddress,
            DeclaredValue = declaredValue,
            RequestedPickupAt = requestedPickupAt,
            ExpectedDeliveryAt = expectedDeliveryAt,
            Note = note?.Trim(),
            IdempotencyKey = idempotencyKey,
        };
    }
    #endregion

    // ============================================================================
    // Items
    // Manages the goods manifest. Only the Shipment may construct a ShipmentItem
    // (ShipmentItem.Create is internal), and the manifest is frozen once the
    // shipment leaves Draft/Requested.
    // ============================================================================

    #region Items
    public void AddItem(
        Guid productId,
        Guid? variantId,
        string name,
        string? sku,
        Quantity quantity,
        decimal? weightKg = null)
    {
        EnsureManifestEditable();

        var lineNo = Items.Count == 0 ? 1 : Items.Max(i => i.LineNo) + 1;
        Items.Add(ShipmentItem.Create(Id, lineNo, productId, variantId, name, sku, quantity, weightKg));
    }

    public void RemoveItem(long shipmentItemId)
    {
        EnsureManifestEditable();

        var item = Items.FirstOrDefault(i => i.Id == shipmentItemId)
            ?? throw ExceptionFactory.EntityNotFound<ShipmentItem>(shipmentItemId);

        Items.Remove(item);
    }

    private void EnsureManifestEditable()
    {
        if (Status is not (ShipmentStatus.Draft or ShipmentStatus.Requested))
            throw ExceptionFactory.InvalidStatus($"Cannot change the manifest of a shipment in {Status} status.");
    }
    #endregion

    // ============================================================================
    // Packages
    // Manages how the manifest is physically boxed. A Package groups ShipmentItem
    // quantities; only the Shipment may construct one.
    // ============================================================================

    #region Packages
    public Package AddPackage(
        string packageCode,
        PackageType packageType,
        decimal weightKg,
        PackageDimensions? dimensions = null)
    {
        EnsureManifestEditable();

        if (Packages.Any(p => p.PackageCode.Equals(packageCode, StringComparison.OrdinalIgnoreCase)))
            throw ExceptionFactory.Duplicate($"Package code '{packageCode}' is already used on this shipment.");

        var package = Package.Create(Id, packageCode, packageType, weightKg, dimensions);
        Packages.Add(package);

        return package;
    }

    public void RemovePackage(Guid packageId)
    {
        EnsureManifestEditable();

        var package = Packages.FirstOrDefault(p => p.Id == packageId)
            ?? throw ExceptionFactory.EntityNotFound<Package>(packageId);

        Packages.Remove(package);
    }
    #endregion

    // ============================================================================
    // Status & lifecycle
    // Drives the intention's own state machine. Execution progress is reported by
    // whichever Transportation is currently running; every transition also appends
    // an immutable ShipmentEvent so the timeline is reconstructable.
    // ============================================================================

    #region Status & lifecycle
    public void Request()
    {
        if (Status != ShipmentStatus.Draft)
            throw ExceptionFactory.InvalidStatus($"Cannot request a shipment in {Status} status.");

        if (Items.Count == 0)
            throw ExceptionFactory.EmptyCollection("A shipment must contain at least one item before it is requested.");

        Transition(ShipmentStatus.Requested, "Shipment requested.");
    }

    public void MarkPlanned()
    {
        if (Status is not (ShipmentStatus.Requested or ShipmentStatus.Failed))
            throw ExceptionFactory.InvalidStatus($"Cannot plan a shipment in {Status} status.");

        Transition(ShipmentStatus.Planned, "Transportation planned for shipment.");
    }

    public void MarkInTransit()
    {
        if (Status is not (ShipmentStatus.Planned or ShipmentStatus.InTransit))
            throw ExceptionFactory.InvalidStatus($"Cannot move a shipment in {Status} status to transit.");

        Transition(ShipmentStatus.InTransit, "Shipment in transit.");
    }

    public void MarkDelivered()
    {
        if (Status != ShipmentStatus.InTransit)
            throw ExceptionFactory.InvalidStatus($"Cannot deliver a shipment in {Status} status.");

        Transition(ShipmentStatus.Delivered, "Shipment delivered.");
    }

    /// <summary>
    /// A failed execution attempt. Deliberately non-terminal for the *intention*: a new
    /// Transportation can be created and the shipment re-planned (see MarkPlanned, which accepts
    /// Failed as an input state) - that retry-without-recreating rule is the whole reason
    /// Shipment and Transportation are separate aggregates.
    /// </summary>
    public void MarkFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw ExceptionFactory.RequiredField("A failure reason is required.");

        if (Status is ShipmentStatus.Delivered or ShipmentStatus.Cancelled)
            throw ExceptionFactory.InvalidStatus($"Cannot fail a shipment in {Status} status.");

        FailureReason = reason.Trim();
        Transition(ShipmentStatus.Failed, reason.Trim());
    }

    public void MarkReturned(string reason)
    {
        if (Status is not (ShipmentStatus.InTransit or ShipmentStatus.Failed or ShipmentStatus.Delivered))
            throw ExceptionFactory.InvalidStatus($"Cannot return a shipment in {Status} status.");

        Transition(ShipmentStatus.Returned, reason);
    }

    public void Cancel(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw ExceptionFactory.RequiredField("A cancellation reason is required.");

        if (Status is ShipmentStatus.Delivered or ShipmentStatus.Returned)
            throw ExceptionFactory.InvalidStatus($"Cannot cancel a shipment in {Status} status.");

        if (Status == ShipmentStatus.Cancelled)
            throw ExceptionFactory.InvalidStatus("Shipment is already cancelled.");

        CancellationReason = reason.Trim();
        Transition(ShipmentStatus.Cancelled, reason.Trim());
    }

    private void Transition(ShipmentStatus status, string description)
    {
        Status = status;
        Events.Add(ShipmentEvent.Record(Id, status, description));
    }
    #endregion
}
