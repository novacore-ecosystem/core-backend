namespace NovaCore.Shipping.Domain.Entities.Shipments;

/// <summary>
/// A physical box/pallet/envelope a Shipment's goods are packed into. Belongs to the Shipment
/// aggregate (only Shipment may construct one) and in turn owns its PackageItem lines, which say
/// how much of each ShipmentItem went into this particular container.
/// </summary>
public sealed class Package : BaseEntity<Guid>, IAuditable
{
    public Guid ShipmentId { get; private set; }

    /// <summary>Human-readable label printed on the box - unique within its Shipment, not globally.</summary>
    public string PackageCode { get; private set; } = string.Empty;
    public PackageType PackageType { get; private set; }
    public decimal WeightKg { get; private set; }
    public PackageDimensions? Dimensions { get; private set; }

    public ICollection<PackageItem> Items { get; private set; } = [];

    private Package() { }

    internal static Package Create(
        Guid shipmentId,
        string packageCode,
        PackageType packageType,
        decimal weightKg,
        PackageDimensions? dimensions = null)
    {
        if (string.IsNullOrWhiteSpace(packageCode))
            throw ExceptionFactory.RequiredField("Package code cannot be empty.");

        if (weightKg <= 0)
            throw ExceptionFactory.InvalidRange("Package weight must be greater than zero.");

        return new Package
        {
            Id = Guid.CreateVersion7(),
            ShipmentId = shipmentId,
            PackageCode = packageCode.Trim().ToUpperInvariant(),
            PackageType = packageType,
            WeightKg = weightKg,
            Dimensions = dimensions,
        };
    }

    public void PackItem(long shipmentItemId, Quantity quantity)
    {
        if (Items.Any(i => i.ShipmentItemId == shipmentItemId))
            throw ExceptionFactory.Duplicate("This shipment item is already packed into this package.");

        Items.Add(PackageItem.Create(Id, shipmentItemId, quantity));
    }

    public void UnpackItem(long shipmentItemId)
    {
        var item = Items.FirstOrDefault(i => i.ShipmentItemId == shipmentItemId)
            ?? throw ExceptionFactory.EntityNotFound<PackageItem>(shipmentItemId);

        Items.Remove(item);
    }
}
