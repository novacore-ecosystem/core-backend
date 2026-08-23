using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Content.Application.Features.Contents.Commands.CreateContentVersion;

namespace NovaCore.Content.API.Endpoints.Contents;

public sealed record CreateContentVersionRequest(string? Language, string Title, string Summary, string Body);

public sealed class CreateContentVersionEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Create Content Version",
        "",
        "Creates a new draft version on an existing Content, carrying the given language's",
        "content. Becomes the Content's current (draft) version; the previously-published",
        "version, if any, is left untouched.",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
        "- **404**: The content item does not exist",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/contents/{contentId}/versions", Handle)
            .WithTags("Contents")
            .RequireAuthorization()
            .WithName("CreateContentVersion")
            .WithDisplayName("Create Content Version API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreateContentVersionResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid contentId,
        [FromBody] CreateContentVersionRequest request,
        [FromServices] ISender sender,
        [FromServices] ICurrentUserService currentUser,
        CancellationToken ct = default)
    {
        var createdBy = currentUser.GetUserId() ?? throw new UnauthorizedException();

        var command = new CreateContentVersionCommand(
            contentId,
            request.Language?.Trim() ?? string.Empty,
            request.Title.Trim(),
            request.Summary.Trim(),
            request.Body,
            createdBy);

        var response = await sender.Send(command, ct);

        return Results.Created($"/contents/{contentId}/versions/{response.VersionId}",
            ApiResponse<CreateContentVersionResponse>.Ok(response));
    }
}
