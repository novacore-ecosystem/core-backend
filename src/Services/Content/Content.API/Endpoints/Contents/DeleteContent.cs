using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Content.Application.Features.Contents.Commands.DeleteContent;

namespace NovaCore.Content.API.Endpoints.Contents;

public sealed class DeleteContentEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Delete Content",
        "",
        "Soft-deletes a Content item. The item is hidden from every normal read/list API but",
        "keeps its identity, versions, and localizations intact - it can be brought back with",
        "Restore Content until the 7-day hard-delete retention job permanently removes it.",
        "",
        "### Error Responses",
        "- **404**: Content not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/contents/{contentId}", Handle)
            .WithTags("Contents")
            .RequireAuthorization()
            .WithName("DeleteContent")
            .WithDisplayName("Delete Content API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<DeleteContentResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid contentId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new DeleteContentCommand(contentId);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<DeleteContentResponse>.Ok(response));
    }
}
