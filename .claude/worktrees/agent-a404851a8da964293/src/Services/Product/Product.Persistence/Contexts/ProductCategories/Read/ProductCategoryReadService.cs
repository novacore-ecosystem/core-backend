using NovaCore.Product.Application.Abstractions.Persistence.ProductCategories;
using NovaCore.Product.Persistence.Engine;

namespace NovaCore.Product.Persistence.Contexts.ProductCategories.Read;

public sealed class ProductCategoryReadService(ProductDbContext dbContext) : IProductCategoryReadService
{
    public async Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.ProductCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Guid[]> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        return await dbContext.ProductCategories
            .AsNoTracking()
            .Where(pc => ids.Contains(pc.Id))
            .Select(pc => pc.Id)
            .ToArrayAsync(ct);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        // Compare the whole value-converted property, not .Value on it - see ProductReadService.CodeExistsAsync.
        var normalized = CategoryCode.Create(code);
        return await dbContext.ProductCategories
            .AsNoTracking()
            .AnyAsync(c => c.Code == normalized, ct);
    }

    public async Task<bool> HasChildrenAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await dbContext.ProductCategories
            .AsNoTracking()
            .AnyAsync(c => c.ParentCategoryId == categoryId, ct);
    }

    public async Task<IReadOnlyList<Guid>> GetChildIdsAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await dbContext.ProductCategories
            .AsNoTracking()
            .Where(c => c.ParentCategoryId == categoryId)
            .Select(c => c.Id)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductCategory>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.ProductCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }
}
