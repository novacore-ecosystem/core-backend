using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Promotions.Repositories;

public sealed class PromotionRepo(PromotionDbContext dbContext)
    : PromotionBaseRepository<PromotionEntity, Guid>(dbContext), IPromotionRepository
{
}
