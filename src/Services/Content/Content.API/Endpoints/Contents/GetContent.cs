using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Content.Application.Features.Contents.Queries.GetContentById;

namespace NovaCore.Content.API.Endpoints.Contents;

public sealed class GetContentEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Content Details",
        "",
        "Retrieves a Content item by id, including its full version history.",
        "",
        "### Route Parameters",
        "- **contentId**: Unique identifier of the content item (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Content not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/contents/{contentId}", Handle)
            .WithTags("Contents")
            .RequireAuthorization()
            .WithName("GetContent")
            .WithDisplayName("Get Content API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetContentByIdResult>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid contentId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetContentByIdQuery(contentId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetContentByIdResult>.Ok(response));
    }
}
