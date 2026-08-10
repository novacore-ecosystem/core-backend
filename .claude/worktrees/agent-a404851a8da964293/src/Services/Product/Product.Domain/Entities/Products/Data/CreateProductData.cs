namespace NovaCore.Product.Domain.Entities.Products.Data;

public sealed record CreateProductData(
    string Name,
    string ShortDesc,
    string Desc,
    Slug Slug,
    ProductType Type,
    IEnumerable<CreateVariantData> Variants,
    Guid? BrandId = null,
    Guid? CategoryId = null,
    IEnumerable<Guid>? TagIds = null,
    ProductStatus Status = ProductStatus.Draft,
    bool IsFeatured = false,
    bool IsVisible = true,
    int DisplayOrder = 0,
    ProductMetadata? Metadata = null);
