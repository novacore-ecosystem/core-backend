using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Content.Application.Features.Contents.Commands.RestoreContentVersion;

namespace NovaCore.Content.API.Endpoints.Contents;

public sealed class RestoreContentVersionEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Restore Content Version",
        "",
        "Restores a prior version's full language set into a brand-new current version.",
        "History is never overwritten - this always creates a new version rather than",
        "reverting the existing one.",
        "",
        "### Error Responses",
        "- **404**: The content item or the referenced version does not exist",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/contents/{contentId}/versions/{versionId}/restore", Handle)
            .WithTags("Contents")
            .RequireAuthorization()
            .WithName("RestoreContentVersion")
            .WithDisplayName("Restore Content Version API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<RestoreContentVersionResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid contentId,
        [FromRoute] Guid versionId,
        [FromServices] ISender sender,
        [FromServices] ICurrentUserService currentUser,
        CancellationToken ct = default)
    {
        var restoredBy = currentUser.GetUserId() ?? throw new UnauthorizedException();

        var command = new RestoreContentVersionCommand(contentId, versionId, restoredBy);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<RestoreContentVersionResponse>.Ok(response));
    }
}
