using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Products.Data;

public sealed record CreateBundleItemData(
    Guid BundleVariantId,
    Guid ProductId,
    Guid VariantId,
    Quantity Quantity,
    int DisplayOrder = 0);
