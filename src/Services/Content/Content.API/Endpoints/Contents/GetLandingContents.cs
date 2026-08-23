using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Content.Application.Features.Contents.Queries.GetLandingContents;

namespace NovaCore.Content.API.Endpoints.Contents;

public sealed class GetLandingContentsEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Landing Contents",
        "",
        "Public, cursor-paginated list of published content for the landing page - resolved to",
        "one language via request-language-then-default fallback. Never returns drafts, deleted",
        "content, or version history.",
        "",
        "### Query Parameters",
        "- **contentTypeId**: Filter to one content type (optional)",
        "- **language**: Preferred language, falls back to the service default (optional)",
        "- **cursor**: Opaque pagination cursor from a previous response's `nextCursor` (optional)",
        "- **limit**: Page size, 1-100 (default 20)",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/contents/landing", Handle)
            .WithTags("Contents")
            .AllowAnonymous()
            .WithName("GetLandingContents")
            .WithDisplayName("Landing Contents API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CursorPaginatedResult<GetLandingContentsItemResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromQuery] Guid? contentTypeId,
        [FromQuery] string? language,
        [FromQuery] string? cursor,
        [FromQuery] int limit,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetLandingContentsQuery(contentTypeId, language, cursor, limit);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<CursorPaginatedResult<GetLandingContentsItemResponse>>.Ok(response));
    }
}
