using NovaCore.Product.Persistence.Engine;

namespace NovaCore.Product.Persistence.Contexts.ProductTags.Repositories;

public sealed class ProductTagRepo(ProductDbContext dbContext)
    : ProductBaseRepository<ProductTag>(dbContext), IProductTagRepository
{
}
