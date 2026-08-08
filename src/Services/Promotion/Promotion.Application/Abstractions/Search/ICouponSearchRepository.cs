namespace NovaCore.Promotion.Application.Abstractions.Search;

/// <summary>
/// Query-only access to the Coupon Search index. Deliberately exposes no Add/Update/Delete -
/// indexing is ICouponSearchIndexer's responsibility, never this repository's. See
/// docs/promotion-service/search/search-strategy.md.
/// </summary>
public interface ICouponSearchRepository
{
    Task<(IReadOnlyList<CouponSearchDocument> Items, long TotalCount)> SearchAsync(
        CouponSearchCriteria criteria, CancellationToken ct = default);
}
