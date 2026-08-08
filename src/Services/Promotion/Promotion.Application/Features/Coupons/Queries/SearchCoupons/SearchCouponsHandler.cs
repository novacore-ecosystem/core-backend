using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Context;

using NovaCore.Promotion.Application.Abstractions.Search;

namespace NovaCore.Promotion.Application.Features.Coupons.Queries.SearchCoupons;

/// <summary>
/// Thin: builds CouponSearchCriteria and delegates to ICouponSearchRepository - no Elasticsearch
/// DSL, index names, or analyzer knowledge here (that stays CouponSearchRepository's, Phase 3.4).
/// Status/Visibility/AvailableAsOf are fixed to "publicly discoverable right now", never taken
/// from the request - public search is not eligibility (see Phase 4.3 brief), so a caller cannot
/// ask to see Draft/Cancelled/Expired Coupons. TenantId comes from RequestContext.Current, the
/// same ambient mechanism EF's own tenant query filter and TenantAssignmentInterceptor use - never
/// a caller-supplied value.
/// </summary>
public sealed class SearchCouponsHandler(ICouponSearchRepository searchRepo)
    : IQueryHandler<SearchCouponsQuery, PaginatedResult<SearchCouponsItemResponse>>
{
    public async Task<PaginatedResult<SearchCouponsItemResponse>> Handle(
        SearchCouponsQuery request, CancellationToken ct = default)
    {
        var criteria = new CouponSearchCriteria(
            RequestContext.Current.TenantId ?? Guid.Empty,
            request.Search,
            nameof(CouponStatus.Active),
            nameof(CouponVisibility.Public),
            DateTime.UtcNow,
            request.SortBy,
            request.SortDescending,
            request.Page,
            request.PageSize);

        var (items, totalCount) = await searchRepo.SearchAsync(criteria, ct);

        var mapped = items
            .Select(d => new SearchCouponsItemResponse(
                d.CouponId, d.Code, d.Name, d.Description, d.TranslatedNames,
                d.StartTime, d.EndTime, d.TimeZone, d.UpdatedAt))
            .ToList();

        return PaginatedResult<SearchCouponsItemResponse>.Create(mapped, request.Page, request.PageSize, (int)totalCount);
    }
}
