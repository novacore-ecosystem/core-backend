namespace NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;

public interface ICouponWriteService
{
    /// <summary>Commits via bare SaveChangesAsync, matching CreateProductCategoryHandler's precedent shape.</summary>
    Task CreateAsync(Coupon coupon, CancellationToken ct = default);

    Task UpdateDetailsAsync(
        Guid couponId,
        string name,
        string? description,
        DateTime startTime,
        DateTime endTime,
        string timeZone,
        CouponVisibility visibility,
        int? maxUsage,
        int? maxUsagePerUser,
        CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync - a single, simple mutation, same shape as DeleteProductCategoryHandler's DeleteAsync.</summary>
    Task DisableAsync(Guid couponId, CancellationToken ct = default);

    /// <summary>Upsert - mirrors Coupon.Translate's own upsert behavior. Commits via bare SaveChangesAsync.</summary>
    Task TranslateAsync(
        Guid couponId,
        string languageCode,
        string name,
        string? description,
        CancellationToken ct = default);
}
