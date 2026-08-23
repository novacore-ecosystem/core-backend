using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Content.Application.Features.Contents.Commands.UpdateContentDraft;

namespace NovaCore.Content.API.Endpoints.Contents;

public sealed record UpdateContentDraftRequest(string? Language, string Title, string Summary, string Body);

public sealed class UpdateContentDraftEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Update Content Draft",
        "",
        "Updates one language's content on a specific version. Only draft (non-published,",
        "non-archived) versions can be edited - publish a new version to change a live one.",
        "",
        "### Error Responses",
        "- **400**: Invalid request, validation failed, or the version is not editable",
        "- **404**: The content item or the referenced version does not exist",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/contents/{contentId}/versions/{versionId}/draft", Handle)
            .WithTags("Contents")
            .RequireAuthorization()
            .WithName("UpdateContentDraft")
            .WithDisplayName("Update Content Draft API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<UpdateContentDraftResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid contentId,
        [FromRoute] Guid versionId,
        [FromBody] UpdateContentDraftRequest request,
        [FromServices] ISender sender,
        [FromServices] ICurrentUserService currentUser,
        CancellationToken ct = default)
    {
        var updatedBy = currentUser.GetUserId() ?? throw new UnauthorizedException();

        var command = new UpdateContentDraftCommand(
            contentId,
            versionId,
            request.Language?.Trim() ?? string.Empty,
            request.Title.Trim(),
            request.Summary.Trim(),
            request.Body,
            updatedBy);

        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<UpdateContentDraftResponse>.Ok(response));
    }
}
