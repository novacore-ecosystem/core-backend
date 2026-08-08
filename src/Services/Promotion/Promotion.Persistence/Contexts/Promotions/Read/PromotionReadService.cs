using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;
using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Promotions.Read;

public sealed class PromotionReadService(PromotionDbContext dbContext) : IPromotionReadService
{
}
