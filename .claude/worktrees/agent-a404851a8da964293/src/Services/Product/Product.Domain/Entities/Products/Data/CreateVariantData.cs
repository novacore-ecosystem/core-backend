using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Products.Data;

public sealed record CreateVariantData(
    Guid ProductId,
    Sku Sku,
    string Name,
    int DisplayOrder,
    Barcode? Barcode = null,
    Weight? Weight = null,
    bool IsActive = true,
    bool IsDefault = false,
    Guid? ProductIdentifierId = null,
    Guid? ProductShippingId = null,
    Guid? WarrantyPolicyId = null,
    VariantMetadata? Metadata = null,
    IEnumerable<Guid>? SelectedOptionValueIds = null,
    CreateBundleItemData[]? BundleItems = null);
