using NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;
using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Coupons.Read;

public sealed class CouponReadService(PromotionDbContext dbContext) : ICouponReadService
{
}
