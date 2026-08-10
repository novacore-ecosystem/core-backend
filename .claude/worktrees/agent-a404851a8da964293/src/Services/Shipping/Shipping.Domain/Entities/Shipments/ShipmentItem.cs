namespace NovaCore.Shipping.Domain.Entities.Shipments;

/// <summary>
/// One line of a Shipment's goods manifest. Product/variant ids are opaque references to
/// ProductService data - name and sku are denormalized snapshots so the manifest stays readable
/// even after the catalog changes. Own table/PK, FK back to Shipment; only Shipment may
/// construct one.
/// </summary>
public sealed class ShipmentItem : BaseEntity<long>, IAuditable
{
    public Guid ShipmentId { get; private set; }
    public int LineNo { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? VariantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Sku { get; private set; }
    public Quantity Quantity { get; private set; } = default!;

    /// <summary>Per-unit weight in kilograms when the source module supplies it - null when unknown.</summary>
    public decimal? WeightKg { get; private set; }

    private ShipmentItem() { }

    internal static ShipmentItem Create(
        Guid shipmentId,
        int lineNo,
        Guid productId,
        Guid? variantId,
        string name,
        string? sku,
        Quantity quantity,
        decimal? weightKg = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Shipment item name cannot be empty.");

        if (quantity.Value < 1)
            throw ExceptionFactory.InvalidRange("Shipment item quantity must be at least 1.");

        if (weightKg is < 0)
            throw ExceptionFactory.InvalidRange("Shipment item weight cannot be negative.");

        return new ShipmentItem
        {
            ShipmentId = shipmentId,
            LineNo = lineNo,
            ProductId = productId,
            VariantId = variantId,
            Name = name.Trim(),
            Sku = sku?.Trim(),
            Quantity = quantity,
            WeightKg = weightKg,
        };
    }
}
