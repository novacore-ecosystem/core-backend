namespace NovaCore.Promotion.Persistence.Contexts.Coupons.Search;

/// <summary>
/// The only place the literal Coupon Search index name lives. This is an ES alias, not a concrete
/// index - the real index behind it is versioned and swapped atomically on every rebuild (see
/// NovaCore.BuildingBlock.Search.Indexing.ElasticsearchIndexer{TDocument}).
/// </summary>
public static class CouponSearchIndexNames
{
    public const string Default = "coupon-search";
}
