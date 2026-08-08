using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Coupons.Repositories;

public sealed class CouponRepo(PromotionDbContext dbContext)
    : PromotionBaseRepository<Coupon, Guid>(dbContext), ICouponRepository
{
}
