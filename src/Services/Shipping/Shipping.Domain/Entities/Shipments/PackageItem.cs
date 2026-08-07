namespace NovaCore.Shipping.Domain.Entities.Shipments;

/// <summary>How much of one ShipmentItem was packed into one Package. Own table/PK, FK to Package; only Package may construct one.</summary>
public sealed class PackageItem : BaseEntity<long>, IAuditable
{
    public Guid PackageId { get; private set; }
    public long ShipmentItemId { get; private set; }
    public Quantity Quantity { get; private set; } = default!;

    private PackageItem() { }

    internal static PackageItem Create(Guid packageId, long shipmentItemId, Quantity quantity)
    {
        if (quantity.Value < 1)
            throw ExceptionFactory.InvalidRange("Packed quantity must be at least 1.");

        return new PackageItem
        {
            PackageId = packageId,
            ShipmentItemId = shipmentItemId,
            Quantity = quantity,
        };
    }
}
