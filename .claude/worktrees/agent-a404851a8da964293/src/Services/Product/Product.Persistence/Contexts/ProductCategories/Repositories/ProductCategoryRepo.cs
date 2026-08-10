using NovaCore.Product.Persistence.Engine;

namespace NovaCore.Product.Persistence.Contexts.ProductCategories.Repositories;

public sealed class ProductCategoryRepo(ProductDbContext dbContext)
    : ProductBaseRepository<ProductCategory>(dbContext), IProductCategoryRepository
{
}
