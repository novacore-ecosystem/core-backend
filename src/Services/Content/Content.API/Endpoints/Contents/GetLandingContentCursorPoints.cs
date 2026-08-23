using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Content.Application.Features.Contents.Queries.GetLandingContentCursorPoints;

namespace NovaCore.Content.API.Endpoints.Contents;

public sealed class GetLandingContentCursorPointsEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Landing Content Cursor Points",
        "",
        "Same filters as Landing Contents, but returns only the cursor for the end of each page",
        "instead of full content - meant to be pre-fetched once in the background per search",
        "session, then used to jump straight to page N via Landing Contents' `cursor` param",
        "without offset pagination. Approximate: if content changes between this call and a",
        "later page request, a record may shift pages - acceptable for a search session, not a",
        "consistency guarantee.",
        "",
        "### Query Parameters",
        "- **contentTypeId**: Filter to one content type (optional)",
        "- **language**: Preferred language, falls back to the service default (optional)",
        "- **pageSize**: Items per page, 1-100 (default 20)",
        "- **pageCount**: How many pages' cursor points to compute, 1-50 (default 5)",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/contents/landing/cursor-points", Handle)
            .WithTags("Contents")
            .AllowAnonymous()
            .WithName("GetLandingContentCursorPoints")
            .WithDisplayName("Landing Content Cursor Points API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetLandingContentCursorPointsResult>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromQuery] Guid? contentTypeId,
        [FromQuery] string? language,
        [FromQuery] int pageSize,
        [FromQuery] int pageCount,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetLandingContentCursorPointsQuery(contentTypeId, language, pageSize, pageCount);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetLandingContentCursorPointsResult>.Ok(response));
    }
}
