namespace NovaCore.Promotion.Application.Abstractions.Search;

/// <summary>
/// Write-only access to the Coupon Search index - the only place Promotion's search sync/rebuild
/// code is allowed to mutate Elasticsearch. Wraps NovaCore.BuildingBlock.Search's generic indexer
/// with the Coupon index name/mapping. See docs/promotion-service/search/search-strategy.md.
/// </summary>
public interface ICouponSearchIndexer
{
    /// <summary>Idempotent - creates the index+mapping only if missing. Called on every service startup.</summary>
    Task EnsureIndexAsync(CancellationToken ct = default);

    /// <summary>Drops and recreates the index - used only by a future rebuild flow.</summary>
    Task RecreateIndexAsync(CancellationToken ct = default);

    Task IndexAsync(CouponSearchDocument document, CancellationToken ct = default);

    Task DeleteAsync(Guid couponId, CancellationToken ct = default);

    Task BulkIndexAsync(IEnumerable<CouponSearchDocument> documents, CancellationToken ct = default);
}
