using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Products;

/// <summary>
/// Owned child of Variant,
/// referenced back via Variant.ProductIdentifierId - the full set of trade identifiers for a variant.
/// </summary>
public sealed class ProductIdentifier : BaseEntity<Guid>, ITenantEntity
{
    public Guid VariantId { get; private set; }
    public ProductVariant Variant { get; private set; } = default!;
    public Sku Sku { get; private set; } = null!;
    public Barcode? Barcode { get; private set; }
    public string? Upc { get; private set; }
    public string? Ean { get; private set; }
    public string? Isbn { get; private set; }
    public string? Gtin { get; private set; }
    public string? Mpn { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductIdentifier() { }

    public static ProductIdentifier Create(
        Guid variantId,
        Sku sku,
        Barcode? barcode = null,
        string? upc = null,
        string? ean = null,
        string? isbn = null,
        string? gtin = null,
        string? mpn = null)
    {
        return new ProductIdentifier
        {
            Id = Guid.CreateVersion7(),
            VariantId = variantId,
            Sku = sku,
            Barcode = barcode,
            Upc = upc,
            Ean = ean,
            Isbn = isbn,
            Gtin = gtin,
            Mpn = mpn,
        };
    }

    public void UpdateCoreIdentifiers(Sku sku, Barcode? barcode)
    {
        Sku = sku;
        Barcode = barcode;
    }

    public void UpdateTradeIdentifiers(
        string? upc,
        string? ean,
        string? isbn,
        string? gtin,
        string? mpn)
    {
        Upc = upc;
        Ean = ean;
        Isbn = isbn;
        Gtin = gtin;
        Mpn = mpn;
    }
}
