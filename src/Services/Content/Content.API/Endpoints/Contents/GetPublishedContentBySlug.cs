using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Content.Application.Features.Contents.Queries.GetPublishedContentBySlug;

namespace NovaCore.Content.API.Endpoints.Contents;

public sealed class GetPublishedContentBySlugEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Published Content",
        "",
        "Public read by slug - resolves the currently published version only, in the requested",
        "language falling back to the service default. Never exposes drafts, deleted content,",
        "or other versions.",
        "",
        "### Query Parameters",
        "- **language**: Preferred language, falls back to the service default (optional)",
        "",
        "### Error Responses",
        "- **404**: Not found, not published, or deleted",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/contents/published/{slug}", Handle)
            .WithTags("Contents")
            .AllowAnonymous()
            .WithName("GetPublishedContentBySlug")
            .WithDisplayName("Get Published Content API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetPublishedContentBySlugResult>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] string slug,
        [FromQuery] string? language,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetPublishedContentBySlugQuery(slug, language);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetPublishedContentBySlugResult>.Ok(response));
    }
}
