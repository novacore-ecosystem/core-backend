using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;
using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts.Promotions.Read;

public sealed class PromotionReadService(PromotionDbContext dbContext) : IPromotionReadService
{
    public async Task<PromotionEntity?> GetByIdAsync(Guid promotionId, CancellationToken ct = default)
    {
        return await dbContext.Promotions
            .AsNoTracking()
            .Include(p => p.Benefits)
            .Include(p => p.Targets)
            .Include(p => p.Constraints)
            .Include(p => p.StackingPolicy)
            .Include(p => p.Translations)
            .FirstOrDefaultAsync(p => p.Id == promotionId, ct);
    }

    public async Task<(IReadOnlyList<PromotionEntity> Items, int TotalCount)> SearchAsync(
        PromotionStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = dbContext.Promotions.AsNoTracking();

        if (status is not null)
            query = query.Where(p => p.Status == status.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<PromotionEntity>> GetEvaluationCandidatesAsync(
        string currency,
        CancellationToken ct = default)
    {
        var currencyValue = Currency.Create(currency);

        return await dbContext.Promotions
            .AsNoTracking()
            .Where(p =>
                p.IsEnabled &&
                p.Status == PromotionStatus.Active &&
                p.Currency == currencyValue &&
                (p.ExecutionPolicy == null || p.ExecutionPolicy.Mode == PromotionExecutionMode.Automatic))
            .Include(p => p.Benefits)
            .Include(p => p.Targets)
            .Include(p => p.Constraints)
            .Include(p => p.StackingPolicy)
            .Include(p => p.ExecutionPolicy)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PromotionExclusion>> GetExclusionsAsync(
        IReadOnlyList<Guid> promotionIds,
        CancellationToken ct = default)
    {
        return await dbContext.PromotionExclusions
            .AsNoTracking()
            .Where(e => promotionIds.Contains(e.PromotionId) || promotionIds.Contains(e.ExcludedPromotionId))
            .ToListAsync(ct);
    }
}
