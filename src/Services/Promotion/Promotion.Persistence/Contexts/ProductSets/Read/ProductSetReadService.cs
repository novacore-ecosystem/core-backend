using NovaCore.Promotion.Application.Abstractions.Persistence.ProductSets;
using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.ProductSets.Read;

public sealed class ProductSetReadService(PromotionDbContext dbContext) : IProductSetReadService
{
}
