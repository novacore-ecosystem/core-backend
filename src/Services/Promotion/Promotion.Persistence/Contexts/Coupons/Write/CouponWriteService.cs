using NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;
using NovaCore.Promotion.Persistence.Contexts.Coupons.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Coupons.Write;

public sealed class CouponWriteService(ICouponRepository couponRepo) : ICouponWriteService
{
}
