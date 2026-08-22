using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Content.Application.Features.Contents.Commands.PublishContent;

namespace NovaCore.Content.API.Endpoints.Contents;

public sealed record PublishContentRequest(Guid VersionId);

public sealed class PublishContentEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Publish Content",
        "",
        "Publishes a specific version of a Content item. Never overwrites the current draft -",
        "editing may continue on the current version after this call.",
        "",
        "### Error Responses",
        "- **404**: The content item or the referenced version does not exist",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/contents/{contentId}/publish", Handle)
            .WithTags("Contents")
            .RequireAuthorization()
            .WithName("PublishContent")
            .WithDisplayName("Publish Content API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<PublishContentResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid contentId,
        [FromBody] PublishContentRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new PublishContentCommand(contentId, request.VersionId);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<PublishContentResponse>.Ok(response));
    }
}
