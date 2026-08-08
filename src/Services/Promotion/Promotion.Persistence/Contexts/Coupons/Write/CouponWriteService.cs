using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;
using NovaCore.Promotion.Persistence.Contexts.Coupons.Repositories;

namespace NovaCore.Promotion.Persistence.Contexts.Coupons.Write;

/// <summary>
/// CreateAsync/DisableAsync/TranslateAsync commit via bare SaveChangesAsync themselves (single,
/// simple mutations). UpdateDetailsAsync does not - it stages the mutation only, since
/// UpdateCouponHandler owns the ExecuteTransactionAsync call itself - same split
/// ProductCategoryWriteService uses.
/// </summary>
public sealed class CouponWriteService(
    ICouponRepository couponRepo,
    IUnitOfWork unitOfWork) : ICouponWriteService
{
    public async Task CreateAsync(Coupon coupon, CancellationToken ct = default)
    {
        await couponRepo.AddAsync(coupon, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateDetailsAsync(
        Guid couponId,
        string name,
        string? description,
        DateTime startTime,
        DateTime endTime,
        string timeZone,
        CouponVisibility visibility,
        int? maxUsage,
        int? maxUsagePerUser,
        CancellationToken ct = default)
    {
        await couponRepo.UpdateAsync(couponId, coupon =>
        {
            coupon.UpdateDetails(name, description);
            coupon.Reschedule(startTime, endTime, timeZone);
            coupon.ChangeVisibility(visibility);
            coupon.ChangeUsageLimits(maxUsage, maxUsagePerUser);
        }, ct);
    }

    public async Task DisableAsync(Guid couponId, CancellationToken ct = default)
    {
        await couponRepo.UpdateAsync(couponId, coupon => coupon.Disable(), ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task TranslateAsync(
        Guid couponId,
        string languageCode,
        string name,
        string? description,
        CancellationToken ct = default)
    {
        await couponRepo.UpdateAsync(couponId, coupon =>
            coupon.Translate(LanguageCode.Create(languageCode), name, description), ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
