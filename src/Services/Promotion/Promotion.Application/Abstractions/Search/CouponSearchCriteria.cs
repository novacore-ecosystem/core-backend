namespace NovaCore.Promotion.Application.Abstractions.Search;

/// <summary>
/// Query params for public Coupon Search. TenantId is mandatory (never optional) - tenant
/// isolation is always applied, unlike every other filter here. Extending this later (e.g. a
/// CouponType filter) is just a new optional field - no redesign of the repository/query pipeline.
/// </summary>
public sealed record CouponSearchCriteria(
    Guid TenantId,
    string? Keyword,
    string? Status,
    string? Visibility,
    DateTime? AvailableAsOf,
    string? SortBy,
    bool SortDescending,
    int Page,
    int PageSize);
