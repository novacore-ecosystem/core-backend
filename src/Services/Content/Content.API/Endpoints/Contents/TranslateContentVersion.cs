using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Content.Application.Features.Contents.Commands.TranslateContentVersion;

namespace NovaCore.Content.API.Endpoints.Contents;

public sealed record TranslateContentVersionRequest(string TargetLanguage, string Title, string Summary, string Body);

public sealed class TranslateContentVersionEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Translate Content Version",
        "",
        "Adds (or updates) a target language's content on an existing version, through the same",
        "domain Upsert mechanism ordinary draft editing uses. The source language's content for",
        "this version is available via Get Content, for the client to translate from.",
        "",
        "### Error Responses",
        "- **400**: Invalid request, validation failed, or the version is not editable",
        "- **404**: The content item or the referenced version does not exist",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/contents/{contentId}/versions/{versionId}/translations", Handle)
            .WithTags("Contents")
            .RequireAuthorization()
            .WithName("TranslateContentVersion")
            .WithDisplayName("Translate Content Version API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<TranslateContentVersionResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid contentId,
        [FromRoute] Guid versionId,
        [FromBody] TranslateContentVersionRequest request,
        [FromServices] ISender sender,
        [FromServices] ICurrentUserService currentUser,
        CancellationToken ct = default)
    {
        var translatedBy = currentUser.GetUserId() ?? throw new UnauthorizedException();

        var command = new TranslateContentVersionCommand(
            contentId,
            versionId,
            request.TargetLanguage.Trim(),
            request.Title.Trim(),
            request.Summary.Trim(),
            request.Body,
            translatedBy);

        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<TranslateContentVersionResponse>.Ok(response));
    }
}
