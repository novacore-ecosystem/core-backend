using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.User.Application.Features.Users.Queries.SearchUsers;

namespace NovaCore.User.API.Endpoints;

public sealed class SearchUsersEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Search Users",
        "",
        "Admin/Root only. Paginated, filterable, sortable search, backed by Elasticsearch",
        "(see docs/reference/search.md) - one query, not a mix of Elasticsearch + PostgreSQL.",
        "",
        "### Request Body",
        "- **keyword**: Free-text match against username/email/full name - case-insensitive, accent-insensitive,",
        "  and word-order-insensitive (e.g. \"Van A\" and \"Nguyen Van A\" both match \"Nguyen Van A\") (optional)",
        "- **filters**: `[{ field, operator, value }]` - e.g. `{ \"field\": \"phone\", \"operator\": \"sw\", \"value\": \"0901\" }` for prefix search, `\"operator\": \"ew\"` for suffix search",
        "- **sorts**: `[{ field, direction }]` - direction is `asc`/`desc`",
        "- **page** / **pageSize**: 1-based paging (default 1 / 20)",
        "",
        "### Searchable Fields",
        "- **userName**, **email**: sortable only (not directly filterable - use `keyword` instead)",
        "- **status**: eq/ne/in/nin",
        "- **phone**: sw/ew (prefix/suffix search against normalized digits)",
        "- **createdAt**, **updatedAt**: sortable only",
        "- **role**: eq/ne against each user's role snapshot",
        "",
        "Individual **firstName**/**middleName**/**lastName** filters are no longer supported directly -",
        "the unified `keyword` search supersedes them (and additionally covers middle name, unlike the old",
        "Postgres-backed search).",
        "",
        "### Error Responses",
        "- **400**: Unknown field, operator not allowed for the field, or malformed value",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/search", Handle)
            .WithTags("User")
            .RequirePermissions(Permissions.Users.View)
            .WithName("SearchUsers")
            .WithDisplayName("Search Users API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<PaginatedResult<SearchUsersItemResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromBody] CriteriaRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new SearchUsersQuery(request);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<PaginatedResult<SearchUsersItemResponse>>.Ok(response));
    }
}
