using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Content.Application.Features.Contents.Commands.CreateContent;

namespace NovaCore.Content.API.Endpoints.Contents;

public sealed record CreateContentRequest(
    Guid ContentTypeId,
    string Slug,
    string? Language,
    string Title,
    string Summary,
    string Body,
    ContentVisibility Visibility = ContentVisibility.Private);

public sealed class CreateContentEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Create Content",
        "",
        "Creates a new Content item together with its required first draft version.",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
        "- **404**: The referenced content type does not exist",
        "- **409**: A content item with the same slug already exists",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/contents", Handle)
            .WithTags("Contents")
            .RequireAuthorization()
            .WithName("CreateContent")
            .WithDisplayName("Create Content API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreateContentResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateContentRequest request,
        [FromServices] ISender sender,
        [FromServices] ICurrentUserService currentUser,
        CancellationToken ct = default)
    {
        var createdBy = currentUser.GetUserId() ?? throw new UnauthorizedException();

        var command = new CreateContentCommand(
            request.ContentTypeId,
            request.Slug.Trim(),
            request.Language?.Trim() ?? string.Empty,
            request.Title.Trim(),
            request.Summary.Trim(),
            request.Body,
            createdBy,
            request.Visibility);

        var response = await sender.Send(command, ct);

        return Results.Created($"/contents/{response.ContentId}",
            ApiResponse<CreateContentResponse>.Ok(response));
    }
}
