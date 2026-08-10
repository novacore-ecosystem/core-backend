using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Product.Persistence.Contexts.ProductCategories.Repositories;

public interface IProductCategoryRepository : IRepository<ProductCategory>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
