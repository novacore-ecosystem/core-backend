using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Content.Application.Features.Contents.Commands.RestoreContent;

namespace NovaCore.Content.API.Endpoints.Contents;

public sealed class RestoreContentEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Restore Content",
        "",
        "Brings a soft-deleted Content item back to its previous state. No-op if the item was",
        "already permanently removed by the hard-delete retention job (404).",
        "",
        "### Error Responses",
        "- **400**: Content is not deleted",
        "- **404**: Content not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/contents/{contentId}/restore", Handle)
            .WithTags("Contents")
            .RequireAuthorization()
            .WithName("RestoreContent")
            .WithDisplayName("Restore Content API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<RestoreContentResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid contentId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new RestoreContentCommand(contentId);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<RestoreContentResponse>.Ok(response));
    }
}
