using Elastic.Clients.Elasticsearch.Mapping;

using NovaCore.Product.Application.Abstractions.Search;

namespace NovaCore.Product.Persistence.Contexts.Products.Search.Mapping;

/// <summary>
/// The Product Search index field mapping - the only place ProductSearchDocument's ES schema is
/// defined. Future schema changes only touch this file + ProductSearchProjectionBuilder.
/// </summary>
public static class ProductSearchIndexMapping
{
    public static void Configure(PropertiesDescriptor<ProductSearchDocument> properties)
    {
        properties
            .Keyword(d => d.ProductId)
            .Keyword(d => d.Code)
            .Text(d => d.Name, t => t.Fields(f => f.Keyword("keyword")))
            .Keyword(d => d.Slug)
            .Keyword(d => d.Thumbnail, k => k.Index(false))
            .DoubleNumber(d => d.DefaultPrice)
            .Keyword(d => d.DefaultVariationId)
            .Keyword(d => d.DefaultVariationSku)
            .Text(d => d.VariationNames)
            .Keyword(d => d.VariationIds)
            .Keyword(d => d.CategoryIds)
            .Text(d => d.CategoryNames)
            .Keyword(d => d.TagIds)
            .Text(d => d.TagNames)
            .Keyword(d => d.Status)
            .Date(d => d.UpdatedAt);
    }
}
