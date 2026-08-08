using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Coupons.Queries.SearchCoupons;

namespace NovaCore.Promotion.API.Endpoints.Coupon;

/// <summary>Public Coupon discovery, served entirely from Elasticsearch - see docs/promotion-service/search/search-strategy.md. Distinct from GET /coupons (ListCoupons), the administrative Postgres-backed list.</summary>
public sealed class SearchCouponsEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Search Coupons",
        "",
        "Paginated, fuzzy-searchable public Coupon discovery, served from Elasticsearch. Only",
        "publicly visible, currently active, enabled Coupons are returned - never eligibility",
        "(whether a specific User can apply a Coupon), which is separate Promotion Engine logic.",
        "",
        "### Query Parameters",
        "- **search**: Free-text match against Coupon code/name/translated names, fuzzy-tolerant (optional)",
        "- **sortBy**: name | code | startTime | endTime | updatedAt (optional, default relevance)",
        "- **sortDescending**: Reverse sort order (optional, default false)",
        "- **page**: Page number, 1-based (default 1)",
        "- **pageSize**: Items per page (default 20)",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/coupons/search", Handle)
            .WithTags("Coupon")
            .RequireAuthorization()
            .WithName("SearchCoupons")
            .WithDisplayName("Search Coupons API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<PaginatedResult<SearchCouponsItemResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] bool? sortDescending,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new SearchCouponsQuery(
            search,
            sortBy,
            sortDescending ?? false,
            page is null or <= 0 ? 1 : page.Value,
            pageSize is null or <= 0 ? 20 : pageSize.Value);

        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<PaginatedResult<SearchCouponsItemResponse>>.Ok(response));
    }
}
