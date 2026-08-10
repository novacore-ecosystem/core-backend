namespace NovaCore.Product.Application.Abstractions.Persistence.ProductCategories;

public interface IProductCategoryReadService
{
    Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Guid[]> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);

    Task<bool> HasChildrenAsync(Guid categoryId, CancellationToken ct = default);

    Task<IReadOnlyList<Guid>> GetChildIdsAsync(Guid categoryId, CancellationToken ct = default);

    Task<IReadOnlyList<ProductCategory>> GetAllAsync(CancellationToken ct = default);
}
