using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Product.Application.Features.Products.Commands.AssignProductTag;

namespace NovaCore.Product.API.Endpoints.Product;

public sealed class AssignProductTagEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Assign Product Tag",
        "",
        "Assigns a tag to a product (many-to-many; idempotent).",
        "",
        "### Route Parameters",
        "- **productId**: Unique identifier of the product (required, must be valid GUID)",
        "- **tagId**: Tag to assign (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Product or tag not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/{productId}/tags/{tagId}", Handle)
            .WithTags("Product")
            .RequirePermissions(Permissions.Product.Manage)
            .WithName("AssignProductTag")
            .WithDisplayName("Assign Product Tag API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<AssignProductTagResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromRoute] Guid tagId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new AssignProductTagCommand(productId, tagId), ct);

        return Results.Ok(ApiResponse<AssignProductTagResponse>.Ok(response));
    }
}
