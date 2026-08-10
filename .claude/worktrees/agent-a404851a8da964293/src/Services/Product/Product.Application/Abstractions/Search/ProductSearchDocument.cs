namespace NovaCore.Product.Application.Abstractions.Search;

/// <summary>
/// The Elasticsearch read-model document for Product Search - deliberately not the Product
/// aggregate. Carries only what's needed to search/display a Product List row; optimized for
/// query, not persistence. See docs/reference/search.md.
/// </summary>
public sealed class ProductSearchDocument
{
    public Guid ProductId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Thumbnail { get; set; }
    public decimal? DefaultPrice { get; set; }
    public Guid? DefaultVariationId { get; set; }
    public string? DefaultVariationSku { get; set; }
    public IReadOnlyList<string> VariationNames { get; set; } = [];
    public IReadOnlyList<Guid> VariationIds { get; set; } = [];
    public IReadOnlyList<Guid> CategoryIds { get; set; } = [];
    public IReadOnlyList<string> CategoryNames { get; set; } = [];
    public IReadOnlyList<Guid> TagIds { get; set; } = [];
    public IReadOnlyList<string> TagNames { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
