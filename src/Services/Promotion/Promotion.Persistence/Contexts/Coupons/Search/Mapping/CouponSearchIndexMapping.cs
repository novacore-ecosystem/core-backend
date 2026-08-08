using Elastic.Clients.Elasticsearch.Mapping;

using NovaCore.Promotion.Application.Abstractions.Search;

namespace NovaCore.Promotion.Persistence.Contexts.Coupons.Search.Mapping;

/// <summary>
/// The Coupon Search index field mapping - the only place CouponSearchDocument's ES schema is
/// defined. Code and Name are both Text (analyzed, fuzzy-matchable via CouponSearchRepository)
/// with a "keyword" sub-field for exact match/sort, same multi-field technique ProductSearch uses
/// for its own Name field. Future schema changes only touch this file.
/// </summary>
public static class CouponSearchIndexMapping
{
    public static void Configure(PropertiesDescriptor<CouponSearchDocument> properties)
    {
        properties
            .Keyword(d => d.CouponId)
            .Text(d => d.Code, t => t.Fields(f => f.Keyword("keyword")))
            .Text(d => d.Name, t => t.Fields(f => f.Keyword("keyword")))
            .Text(d => d.Description)
            .Text(d => d.TranslatedNames)
            .Keyword(d => d.Status)
            .Keyword(d => d.Visibility)
            .Date(d => d.StartTime)
            .Date(d => d.EndTime)
            .Keyword(d => d.TimeZone, k => k.Index(false))
            .Boolean(d => d.IsEnabled)
            .Keyword(d => d.TenantId)
            .Date(d => d.UpdatedAt);
    }
}
