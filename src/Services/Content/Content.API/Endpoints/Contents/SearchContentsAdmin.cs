using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Content.Application.Features.Contents.Queries.SearchContentsAdmin;

namespace NovaCore.Content.API.Endpoints.Contents;

public sealed class SearchContentsAdminEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Search Contents (Admin)",
        "",
        "Admin/WCM content list. Filters/sorts on the request body use the existing Criteria",
        "mechanism; pagination is cursor-based (opaque `cursor` query param, not page numbers)",
        "on a fixed CreatedAt-desc order. Never returns the Editor.js body.",
        "",
        "### Query Parameters",
        "- **cursor**: Opaque pagination cursor from a previous response's `nextCursor` (optional)",
        "- **limit**: Page size, 1-100 (default 20)",
        "- **language**: Display language for the list's title column (defaults to the service default)",
        "",
        "### Searchable Fields",
        "- **contentTypeId**: eq/ne/in/nin",
        "- **status**, **visibility**: eq/ne/in/nin",
        "- **createdAt**: eq/ne/gt/gte/lt/lte/between",
        "",
        "### Error Responses",
        "- **400**: Unknown field, operator not allowed for the field, or malformed value",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/contents/admin/search", Handle)
            .WithTags("Contents")
            .RequireAuthorization()
            .WithName("SearchContentsAdmin")
            .WithDisplayName("Search Contents (Admin) API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CursorPaginatedResult<SearchContentsAdminItemResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromBody] CriteriaRequest criteria,
        [FromQuery] string? cursor,
        [FromQuery] int limit,
        [FromQuery] string? language,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new SearchContentsAdminQuery(criteria, cursor, limit, language);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<CursorPaginatedResult<SearchContentsAdminItemResponse>>.Ok(response));
    }
}
