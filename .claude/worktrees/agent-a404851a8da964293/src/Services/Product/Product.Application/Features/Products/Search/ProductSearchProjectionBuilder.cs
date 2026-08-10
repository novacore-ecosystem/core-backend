using NovaCore.Product.Application.Abstractions.Search;

namespace NovaCore.Product.Application.Features.Products.Search;

/// <summary>
/// Integration Event -&gt; Search Document. The only place ProductSearchDocument is assembled -
/// reused by both the live sync path (OnProductSearchSyncRequiredHandler) and the rebuild path
/// (RebuildProductSearchIndexHandler), so future schema changes only touch this one class. See
/// docs/reference/search.md.
/// </summary>
public sealed class ProductSearchProjectionBuilder()
{
    public static async Task<ProductSearchDocument> BuildAsync(ProductEntity product, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return Build(product);
    }

    public static async Task<IReadOnlyList<ProductSearchDocument>> BuildManyAsync(
        IReadOnlyList<ProductEntity> products, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return [.. products.Select(Build)];
    }

    private static ProductSearchDocument Build(ProductEntity product)
    {
        var defaultVariation = product.DefaultVariation;
        var categoryIds = product.CategoryMappings.Select(m => m.CategoryId).ToList();
        var tagIds = product.TagMappings.Select(m => m.TagId).ToList();

        return new ProductSearchDocument
        {
            ProductId = product.Id,
            Code = product.Code.Value,
            Name = product.Name,
            Slug = product.Slug.Value,
            Thumbnail = defaultVariation.Images.FirstOrDefault(),
            DefaultPrice = defaultVariation.Price,
            DefaultVariationId = defaultVariation.Id,
            DefaultVariationSku = defaultVariation.Sku.Value,
            // Active only - a Discontinued/Inactive variation's name shouldn't surface a product
            // in keyword search results the customer can't actually buy that variant from.
            VariationNames = [.. product.Variations
                .Where(v => v.Status == VariantStatus.Active)
                .Select(v => v.Name)
                .Distinct()],
            // Every Active variation's id, not just the Default's - lets SearchProductsHandler
            // compute "is ANY variation in stock" instead of only checking the default.
            VariationIds = [.. product.Variations
                .Where(v => v.Status == VariantStatus.Active)
                .Select(v => v.Id)],
            CategoryIds = categoryIds,
            CategoryNames = [.. product.CategoryMappings.Select(cm => cm.Category.Name)],
            TagIds = tagIds,
            TagNames = [.. product.TagMappings.Select(tm => tm.Tag.Name)],
            // Product itself has no lifecycle status field today - the Default variation's
            // status is the documented stand-in (see docs/reference/search.md).
            Status = defaultVariation.Status.ToString(),
            UpdatedAt = DateTime.UtcNow,
        };
    }
}
