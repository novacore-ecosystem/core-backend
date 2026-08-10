namespace NovaCore.Product.Application.Abstractions.Persistence.ProductCategories;

public interface IProductCategoryWriteService
{
    /// <summary>Commits via bare SaveChangesAsync, matching CreateProductCategoryHandler's current shape.</summary>
    Task CreateAsync(ProductCategory category, CancellationToken ct = default);

    Task UpdateDetailsAsync(Guid id, string name, string description, Guid? parentCategoryId, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync, matching DeleteProductCategoryHandler's current shape.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
