using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.ProductSets.Repositories;

public sealed class ProductSetRepo(PromotionDbContext dbContext)
    : PromotionBaseRepository<ProductSet, Guid>(dbContext), IProductSetRepository
{
}
