namespace NovaCore.Promotion.Application.Abstractions.Search;

/// <summary>
/// The Elasticsearch read-model document for public Coupon search - deliberately not the Coupon
/// aggregate. Carries only what public discovery needs (code/name/translated-name fuzzy matching,
/// status/visibility/date filtering, tenant isolation), never internal accounting, approval, or
/// eligibility configuration. See docs/promotion-service/search/search-strategy.md.
/// </summary>
public sealed class CouponSearchDocument
{
    public Guid CouponId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IReadOnlyList<string> TranslatedNames { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public Guid TenantId { get; set; }
    public DateTime UpdatedAt { get; set; }
}
