using NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;
using NovaCore.Promotion.Persistence.Contexts.Coupons.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Coupons.Read;

public sealed class CouponReadService(ICouponRepository couponRepo) : ICouponReadService
{
}
