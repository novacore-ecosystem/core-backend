using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Product.Application.Features.Products.Queries.SearchProducts;

namespace NovaCore.Product.API.Endpoints.Product;

/// <summary>Served entirely from Elasticsearch - see docs/reference/search.md. Product Detail (GetProduct) still reads Postgres.</summary>
public sealed class SearchProductsEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Search Products",
        "",
        "Paginated, searchable, filterable list of products, served from Elasticsearch.",
        "",
        "### Query Parameters",
        "- **search**: Free-text match against name/code/category/tag names (optional)",
        "- **categoryId** / **tagId**: Filter to products assigned to this category/tag (optional)",
        "- **status**: Filter by variation status - Active/Inactive/Discontinued (optional)",
        "- **sortBy**: name | price | updatedAt (optional, default relevance)",
        "- **sortDescending**: Reverse sort order (optional, default false)",
        "- **page**: Page number, 1-based (default 1)",
        "- **pageSize**: Items per page (default 20)",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", Handle)
            .WithTags("Product")
            .RequireAuthorization()
            .WithName("SearchProducts")
            .WithDisplayName("Search Products API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<PaginatedResult<SearchProductsItemResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromQuery] string? search,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? tagId,
        [FromQuery] string? status,
        [FromQuery] string? sortBy,
        [FromQuery] bool? sortDescending,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new SearchProductsQuery(
            search,
            categoryId,
            tagId,
            status,
            sortBy,
            sortDescending ?? false,
            page is null or <= 0 ? 1 : page.Value,
            pageSize is null or <= 0 ? 20 : pageSize.Value);

        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<PaginatedResult<SearchProductsItemResponse>>.Ok(response));
    }
}
