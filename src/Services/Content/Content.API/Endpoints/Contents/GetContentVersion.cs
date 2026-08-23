using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Content.Application.Features.Contents.Queries.GetContentVersion;

namespace NovaCore.Content.API.Endpoints.Contents;

public sealed class GetContentVersionEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Content Version",
        "",
        "Retrieves a single version's full content - every language it carries, including the",
        "Editor.js JSON body.",
        "",
        "### Error Responses",
        "- **404**: The content item or the referenced version does not exist",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/contents/{contentId}/versions/{versionId}", Handle)
            .WithTags("Contents")
            .RequireAuthorization()
            .WithName("GetContentVersion")
            .WithDisplayName("Get Content Version API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetContentVersionResult>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid contentId,
        [FromRoute] Guid versionId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetContentVersionQuery(contentId, versionId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetContentVersionResult>.Ok(response));
    }
}
