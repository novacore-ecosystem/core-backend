using NovaCore.BuildingBlock.Persistence.Repository;

using NovaCore.Product.Application.Abstractions.Persistence.Products;
using NovaCore.Product.Persistence.Contexts.Products.Repositories;
using NovaCore.Product.Persistence.Engine;

namespace NovaCore.Product.Persistence.Contexts.Products.Read;

public sealed class ProductReadService(
    ProductDbContext dbContext,
    IRepository<ProductEntity, Guid> repo,
    IProductRepository productRepo) : IProductReadService
{
    public async Task<ProductEntity[]> GetAllAsync(
        int skip,
        int take,
        CancellationToken ct = default)
    {
        return await productRepo.GetAllAsync(skip, take, ct);
    }

    public async Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await repo.GetByIdAsync(
            id,
            query => query.AsSplitQuery()
                .Include(p => p.Variations)
                .Include(p => p.CategoryMappings)
                .ThenInclude(cm => cm.Category)
                .Include(p => p.TagMappings)
                .ThenInclude(tm => tm.Tag),
            ct);
    }

    public async Task<Guid[]> GetProductsByTagIdAsync(
        Guid tagId,
        CancellationToken ct = default)
    {
        return await productRepo.GetProductsByTagIdAsync(tagId, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        var normalized = ProductCode.Create(code);
        return await dbContext.Products.AsNoTracking().AnyAsync(p => p.Code == normalized, ct);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeProductId = null, CancellationToken ct = default)
    {
        var normalized = Slug.Create(slug);
        return await dbContext.Products
            .AsNoTracking()
            .AnyAsync(p => p.Slug == normalized && (excludeProductId == null || p.Id != excludeProductId), ct);
    }

    public async Task<bool> SkuExistsAsync(Sku sku, Guid? excludeVariationId = null, CancellationToken ct = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .SelectMany(p => p.Variations)
            .AnyAsync(v => v.Sku.Equals(sku) && (excludeVariationId == null || v.Id != excludeVariationId), ct);
    }

    public async Task<string?> GetProductNameBySkuAsync(string sku, CancellationToken ct = default)
    {
        var normalized = Sku.Create(sku);
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Variations.Any(v => v.Sku == normalized))
            .Select(p => p.Name)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> ExistsWithCategoryAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .AnyAsync(p => p.CategoryMappings.Any(m => m.CategoryId == categoryId), ct);
    }
}
