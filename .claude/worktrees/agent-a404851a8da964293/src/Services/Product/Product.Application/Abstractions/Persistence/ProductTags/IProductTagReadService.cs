namespace NovaCore.Product.Application.Abstractions.Persistence.ProductTags;

public interface IProductTagReadService
{
    Task<ProductTag[]> GetAllAsync(CancellationToken ct = default);

    Task<ProductTag?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Guid[]> GetExistingTagIdsAsync(
        IEnumerable<Guid> tagIds,
        CancellationToken ct = default);

    Task<bool> IsExistAsync(Guid id, CancellationToken ct = default);

    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
}
