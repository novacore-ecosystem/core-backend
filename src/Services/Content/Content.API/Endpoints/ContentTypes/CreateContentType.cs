using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Content.Application.Features.ContentTypes.Commands.CreateContentType;

namespace NovaCore.Content.API.Endpoints.ContentTypes;

public sealed record CreateContentTypeRequest(string Key, string Name, string Description);

public sealed class CreateContentTypeEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Create Content Type",
        "",
        "Creates a new content type (schema) that Content items can be created against.",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
        "- **409**: A content type with the same key already exists",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/content-types", Handle)
            .WithTags("ContentTypes")
            .RequireAuthorization()
            .WithName("CreateContentType")
            .WithDisplayName("Create Content Type API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreateContentTypeResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateContentTypeRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new CreateContentTypeCommand(
            request.Key.Trim(),
            request.Name.Trim(),
            request.Description.Trim());

        var response = await sender.Send(command, ct);

        return Results.Created($"/content-types/{response.ContentTypeId}",
            ApiResponse<CreateContentTypeResponse>.Ok(response));
    }
}
