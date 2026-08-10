using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence.Repository;

using NovaCore.Product.Application.Abstractions.Persistence.ProductTags;

namespace NovaCore.Product.Persistence.Contexts.ProductTags.Write;

public sealed class ProductTagWriteService(
    IRepository<ProductTag, Guid> repo,
    IUnitOfWork unitOfWork) : IProductTagWriteService
{
    public async Task CreateAsync(ProductTag tag, CancellationToken ct = default)
    {
        await repo.AddAsync(tag, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateTagNameAsync(Guid id, string tagName, CancellationToken ct = default)
    {
        await repo.UpdateAsync(id, productTag =>
        {
            productTag.Rename(tagName);
        }, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await repo.DeleteAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
