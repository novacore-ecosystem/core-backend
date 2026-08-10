using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Product.Application.Features.ProductTags.Commands.CreateProductTag;

namespace NovaCore.Product.API.Endpoints.ProductTag;

public sealed record CreateProductTagRequest(string Code, string Name);

public sealed class CreateProductTagEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Create Product Tag",
        "",
        "Creates a new flat product tag (no hierarchy).",
        "",
        "### Request Body",
        "- **Code**: Unique tag code (required, must be unique)",
        "- **Name**: Tag name (required)",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
        "- **409**: Code already exists",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/tags", Handle)
            .WithTags("ProductTag")
            .RequirePermissions(Permissions.Product.Manage)
            .WithName("CreateProductTag")
            .WithDisplayName("Create Product Tag API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreateProductTagResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateProductTagRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new CreateProductTagCommand(request.Code.Trim(), request.Name.Trim());
        var response = await sender.Send(command, ct);

        return Results.Created($"/tags/{response.ProductTagId}", ApiResponse<CreateProductTagResponse>.Ok(response));
    }
}
