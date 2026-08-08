using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;
using NovaCore.Promotion.Persistence.Contexts.Promotions.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Promotions.Write;

public sealed class PromotionWriteService(IPromotionRepository promotionRepo) : IPromotionWriteService
{
}
