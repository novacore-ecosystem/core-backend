using NovaCore.BuildingBlock.Application.Abstractions.Common;

namespace NovaCore.Promotion.Application.Features.Coupons.Queries.SearchCoupons;

/// <summary>
/// Public Coupon discovery, served entirely from Elasticsearch - never Postgres, matching
/// SearchProducts. Answers "which Coupons can be found?" only - never "can this User apply this
/// Coupon?" (eligibility), which stays Promotion Engine business logic for a later phase. See
/// docs/promotion-service/search/search-strategy.md.
/// </summary>
public sealed record SearchCouponsQuery(
    string? Search,
    string? SortBy,
    bool SortDescending = false,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedResult<SearchCouponsItemResponse>>;

public sealed record SearchCouponsItemResponse(
    Guid CouponId,
    string Code,
    string Name,
    string? Description,
    IReadOnlyList<string> TranslatedNames,
    DateTime StartTime,
    DateTime EndTime,
    string TimeZone,
    DateTime UpdatedAt);
