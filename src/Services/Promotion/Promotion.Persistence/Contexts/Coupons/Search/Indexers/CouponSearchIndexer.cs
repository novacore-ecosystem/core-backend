using NovaCore.BuildingBlock.Search.Abstractions;

using NovaCore.Promotion.Application.Abstractions.Search;
using NovaCore.Promotion.Persistence.Contexts.Coupons.Search.Mapping;

namespace NovaCore.Promotion.Persistence.Contexts.Coupons.Search.Indexers;

/// <summary>
/// ICouponSearchIndexer impl - fixes the Coupon index name/mapping on top of
/// NovaCore.BuildingBlock.Search's generic, reusable IElasticsearchIndexer&lt;&gt;. The name passed
/// through here is an ES alias, not a concrete index - EnsureIndexAsync/RecreateIndexAsync manage
/// the versioned index + alias swap underneath; this class stays unaware of that detail.
/// </summary>
public sealed class CouponSearchIndexer(IElasticsearchIndexer<CouponSearchDocument> indexer) : ICouponSearchIndexer
{
    public Task EnsureIndexAsync(CancellationToken ct = default) =>
        indexer.EnsureIndexAsync(CouponSearchIndexNames.Default, CouponSearchIndexMapping.Configure, ct);

    public Task RecreateIndexAsync(CancellationToken ct = default) =>
        indexer.RecreateIndexAsync(CouponSearchIndexNames.Default, CouponSearchIndexMapping.Configure, ct);

    public Task IndexAsync(CouponSearchDocument document, CancellationToken ct = default) =>
        indexer.IndexAsync(CouponSearchIndexNames.Default, document.CouponId.ToString(), document, ct);

    public Task DeleteAsync(Guid couponId, CancellationToken ct = default) =>
        indexer.DeleteAsync(CouponSearchIndexNames.Default, couponId.ToString(), ct);

    public Task BulkIndexAsync(IEnumerable<CouponSearchDocument> documents, CancellationToken ct = default) =>
        indexer.BulkIndexAsync(
            CouponSearchIndexNames.Default,
            documents.Select(d => (d.CouponId.ToString(), d)),
            ct);
}
