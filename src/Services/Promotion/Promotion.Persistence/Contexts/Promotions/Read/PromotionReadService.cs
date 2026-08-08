using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;
using NovaCore.Promotion.Persistence.Contexts.Promotions.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Promotions.Read;

public sealed class PromotionReadService(IPromotionRepository promotionRepo) : IPromotionReadService
{
}
