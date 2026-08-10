using NovaCore.BuildingBlock.Persistence.Repository;

using NovaCore.Product.Application.Abstractions.Persistence.ProductTags;
using NovaCore.Product.Persistence.Engine;

namespace NovaCore.Product.Persistence.Contexts.ProductTags.Read;

public sealed class ProductTagReadService(
    IRepository<ProductTag, Guid> repo,
    ProductDbContext dbContext) : IProductTagReadService
{
    public async Task<ProductTag[]> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.ProductTags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToArrayAsync(ct);
    }

    public async Task<ProductTag?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await repo.GetByIdAsync(id, ct);
    }

    public async Task<Guid[]> GetExistingTagIdsAsync(
        IEnumerable<Guid> tagIds,
        CancellationToken ct = default)
    {
        return await dbContext.ProductTags
            .AsNoTracking()
            .Where(pt => tagIds.Contains(pt.Id))
            .Select(pt => pt.Id)
            .ToArrayAsync(ct);
    }

    public async Task<bool> IsExistAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.ProductTags
            .AsNoTracking()
            .AnyAsync(pt => pt.Id == id, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        var normalized = TagCode.Create(code);
        return await dbContext.ProductTags
            .AsNoTracking()
            .AnyAsync(t => t.Code == normalized, ct);
    }
}
