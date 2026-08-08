using NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;
using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Coupons.Read;

public sealed class CouponReadService(PromotionDbContext dbContext) : ICouponReadService
{
    public async Task<Coupon?> GetByIdAsync(Guid couponId, CancellationToken ct = default)
    {
        return await dbContext.Coupons
            .AsNoTracking()
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == couponId, ct);
    }

    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var entityCode = EntityCode.Create(code);

        return await dbContext.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == entityCode, ct);
    }

    public async Task<IReadOnlyList<CouponUsage>> GetUsagesByCouponAndUserAsync(
        Guid couponId,
        Guid userId,
        CancellationToken ct = default)
    {
        return await dbContext.CouponUsages
            .AsNoTracking()
            .Where(u => u.CouponId == couponId && u.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Coupon> Items, int TotalCount)> SearchAsync(
        CouponStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = dbContext.Coupons.AsNoTracking();

        if (status is not null)
            query = query.Where(c => c.Status == status.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
