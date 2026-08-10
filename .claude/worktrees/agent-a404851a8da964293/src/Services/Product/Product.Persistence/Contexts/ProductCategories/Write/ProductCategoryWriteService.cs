using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence.Repository;

using NovaCore.Product.Application.Abstractions.Persistence.ProductCategories;

namespace NovaCore.Product.Persistence.Contexts.ProductCategories.Write;

/// <summary>
/// CreateAsync/DeleteAsync commit via bare SaveChangesAsync themselves (matching the original
/// handlers' shape). UpdateDetailsAsync does not - it stages the mutation only, since
/// UpdateProductCategoryHandler owns the ExecuteTransactionAsync call itself.
/// </summary>
public sealed class ProductCategoryWriteService(
    IRepository<ProductCategory, Guid> repo,
    IUnitOfWork unitOfWork) : IProductCategoryWriteService
{
    public async Task CreateAsync(ProductCategory category, CancellationToken ct = default)
    {
        await repo.AddAsync(category, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateDetailsAsync(Guid id, string name, string description, Guid? parentCategoryId, CancellationToken ct = default)
    {
        await repo.UpdateAsync(id, async category =>
        {
            category.UpdateDetails(name, description);
            category.ChangeParent(parentCategoryId);
        }, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await repo.DeleteAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
